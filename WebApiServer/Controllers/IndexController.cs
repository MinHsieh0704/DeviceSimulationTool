using Min_Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace WebApiServer.Controllers
{
    public class IndexController : ApiController
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete]
        public async Task<IHttpActionResult> Handle()
        {
            try
            {
                HttpRequestMessage req = Request;
                Uri reqUri = req.RequestUri;
                HttpMethod reqMethod = req.Method;
                HttpContent reqContent = req.Content;
                HttpRequestHeaders reqHeaders = req.Headers;

                if (Program.basicAuth != null)
                {
                    var auth = reqHeaders.Authorization;
                    if (auth == null)
                    {
                        HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                        res.Headers.Add("WWW-Authenticate", "Basic realm=\"User Visible Realm\"");
                        res.Content = new StringContent("This action requires login.");

                        throw new HttpResponseException(res);
                    }
                    else
                    {
                        string authSource = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Parameter));
                        string account = authSource.Substring(0, authSource.IndexOf(":"));
                        string password = authSource.Substring(authSource.IndexOf(":") + 1, authSource.Length - authSource.IndexOf(":") - 1);

                        if (account != Program.basicAuth?.account || password != Program.basicAuth?.password)
                        {
                            HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                            res.Content = new StringContent("Login failed.");

                            throw new HttpResponseException(res);
                        }
                    }
                }

                List<KeyValuePair<string, IEnumerable<string>>> headers = reqHeaders.ToList().Concat(reqContent.Headers.ToList()).ToList();

                string contentType = headers.Where((n) => n.Key.ToLower() == "content-type" || n.Key.ToLower() == "contenttype").Select((n) => n.Value.FirstOrDefault()).ToList().FirstOrDefault();
                contentType = contentType != null && contentType.IndexOf("multipart/form-data") > -1 ? "multipart/form-data" : contentType;
                contentType = reqMethod == HttpMethod.Get || reqMethod == HttpMethod.Delete ? null : contentType;
                /// "application/json"
                /// "text/plain"
                /// "application/xml"
                /// "text/xml"
                /// "application/x-www-form-urlencoded"
                /// "multipart/form-data"

                JToken content = null;

                string bodyInput = null;

                if (reqMethod == HttpMethod.Get || reqMethod == HttpMethod.Delete)
                {
                    content = new JObject();

                    NameValueCollection queryInput = HttpUtility.ParseQueryString(reqUri.Query);

                    foreach (var key in queryInput)
                    {
                        List<string> keys = Regex.Split(key.ToString(), @"\.").ToList();
                        JToken _JToken = content;
                        for (int i = 0; i < keys.Count(); i++)
                        {
                            if (i == keys.Count() - 1) _JToken[keys[i]] = queryInput[key.ToString()];
                            if (_JToken[keys[i]] == null) _JToken[keys[i]] = new JObject();
                            _JToken = _JToken[keys[i]];
                        }
                    }
                }
                else if (reqMethod == HttpMethod.Post || reqMethod == HttpMethod.Put)
                {
                    switch (contentType)
                    {
                        case "application/json":
                            bodyInput = reqContent.ReadAsStringAsync().Result;
                            if (!string.IsNullOrEmpty(bodyInput))
                            {
                                content = JToken.Parse(bodyInput);
                            }
                            break;
                        case "text/plain":
                        case "application/xml":
                        case "text/xml":
                            bodyInput = reqContent.ReadAsStringAsync().Result;
                            break;
                        case "application/x-www-form-urlencoded":
                            content = new JObject();

                            bodyInput = reqContent.ReadAsStringAsync().Result;
                            if (!string.IsNullOrEmpty(bodyInput))
                            {
                                NameValueCollection queryInput = HttpUtility.ParseQueryString(bodyInput);

                                foreach (var key in queryInput)
                                {
                                    List<string> keys = Regex.Split(key.ToString(), @"\.").ToList();
                                    JToken _JToken = content;
                                    for (int i = 0; i < keys.Count(); i++)
                                    {
                                        if (i == keys.Count() - 1) _JToken[keys[i]] = queryInput[key.ToString()];
                                        if (_JToken[keys[i]] == null) _JToken[keys[i]] = new JObject();
                                        _JToken = _JToken[keys[i]];
                                    }
                                }
                            }
                            break;
                        case "multipart/form-data":
                            content = new JObject();

                            MultipartMemoryStreamProvider formdataInput = reqContent.ReadAsMultipartAsync().Result;
                            for (int i = 0; i < formdataInput.Contents.Count(); i++)
                            {
                                HttpContent data = formdataInput.Contents[i];

                                ContentDispositionHeaderValue disposition = data.Headers.ContentDisposition;
                                if (disposition.FileName == null)
                                {
                                    string key = disposition.Name.Replace("\"", "");
                                    string value = data.ReadAsStringAsync().Result;

                                    content[key] = value;
                                }
                                else
                                {
                                    string key = disposition.Name.Replace("\"", "");
                                    string filename = disposition.FileName.Replace("\"", "");
                                    string fileMediaType = data.Headers.ContentType.MediaType;

                                    content[key] = new JObject()
                                    {
                                        new JProperty("filename", filename),
                                        new JProperty("fileMediaType", fileMediaType),
                                    };
                                }
                            }
                            break;
                        default:
                            return NotFound();
                    }
                }

                string info = "";
                info += $"{{{{newline}}}}    Method: {reqMethod.Method}";
                info += $"{{{{newline}}}}    Path: {reqUri.LocalPath}";
                info += $"{{{{newline}}}}    Headers:";
                foreach (var header in headers)
                {
                    if (header.Value == null) continue;
                    info += $"{{{{newline}}}}        - {header.Key}: {JsonConvert.SerializeObject(header.Value)}";
                }
                info += $"{{{{newline}}}}    Content:";
                switch (contentType)
                {
                    case "application/json":
                        info += IndexController.ContentInfo(content, 0);
                        break;
                    case "text/plain":
                    case "application/xml":
                    case "text/xml":
                        info += (string.IsNullOrEmpty(bodyInput) ? "" : $"{{{{newline}}}}{"".PadLeft(12, ' ')}{Regex.Replace(bodyInput, $"(\r)?\n", $"{{{{newline}}}}{"".PadLeft(12, ' ')}")}");
                        break;
                    case "application/x-www-form-urlencoded":
                        info += IndexController.ContentInfo(content, 0);
                        break;
                    case "multipart/form-data":
                        info += IndexController.ContentInfo(content, 0);
                        break;
                    case null:
                        info += IndexController.ContentInfo(content, 0);
                        break;
                }

                Console.WriteLine($"{info}");

                switch (contentType)
                {
                    case "application/json":
                        return Json(content);
                    case "text/plain":
                    case "application/xml":
                    case "text/xml":
                        return Ok(string.IsNullOrEmpty(bodyInput) ? "" : bodyInput);
                    case "application/x-www-form-urlencoded":
                        return Json(content);
                    case "multipart/form-data":
                        return Json(content);
                    case null:
                        return Json(content);
                    default:
                        return ResponseMessage(new HttpResponseMessage() { StatusCode = HttpStatusCode.NoContent });
                }
            }
            catch (HttpResponseException ex)
            {
                Console.WriteLine($"{ex.Response.StatusCode}, {await ex.Response.Content.ReadAsStringAsync()}");

                return ResponseMessage(ex.Response);
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                Console.WriteLine($"500, {ex.Message}");

                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Content Info
        /// </summary>
        /// <param name="content"></param>
        /// <param name="deep"></param>
        /// <returns></returns>
        public static string ContentInfo(JToken content, int deep)
        {
            try
            {
                JObject _content = new JObject();

                if (content.Type == JTokenType.Array)
                {
                    for (int i = 0; i < content.Count(); i++)
                    {
                        _content.Add(new JProperty(i.ToString(), content[i]));
                    }
                }
                else
                {
                    _content = JObject.FromObject(content);
                }

                string info = "";

                foreach (var input in _content.Properties())
                {
                    JToken jToken = _content[input.Name];
                    switch (jToken.Type)
                    {
                        case JTokenType.Null:
                            info += $"{{{{newline}}}}{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}: null";
                            break;
                        case JTokenType.Object:
                            info += $"{{{{newline}}}}{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}:";
                            info += IndexController.ContentInfo(jToken, deep + 1);
                            break;
                        case JTokenType.Array:
                            info += $"{{{{newline}}}}{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}:";
                            for (int i = 0; i < jToken.Count(); i++)
                            {
                                info += IndexController.ContentInfo(new JObject() { new JProperty(i.ToString(), jToken[i]) }, deep + 1);
                            }
                            break;
                        case JTokenType.Date:
                            DateTime dt = (DateTime)input.Value;
                            var a = TimeZoneInfo.ConvertTimeToUtc(dt);
                            info += $"{{{{newline}}}}{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}: {a.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}";
                            break;
                        default:
                            info += $"{{{{newline}}}}{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}: {input.Value}";
                            break;
                    }
                }

                return info;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
