﻿using Min_Helpers;
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

                JObject content = new JObject();
                if (reqMethod == HttpMethod.Get || reqMethod == HttpMethod.Delete)
                {
                    NameValueCollection input = HttpUtility.ParseQueryString(reqUri.Query);

                    foreach (var key in input)
                    {
                        List<string> keys = Regex.Split(key.ToString(), @"\.").ToList();
                        JToken _JToken = content;
                        for (int i = 0; i < keys.Count(); i++)
                        {
                            if (i == keys.Count() - 1) _JToken[keys[i]] = input[key.ToString()];
                            if (_JToken[keys[i]] == null) _JToken[keys[i]] = new JObject();
                            _JToken = _JToken[keys[i]];
                        }
                    }
                }
                else if (reqMethod == HttpMethod.Post || reqMethod == HttpMethod.Put)
                {
                    string input = reqContent.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(input)) content = JsonConvert.DeserializeObject<JObject>(input);
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
                info += IndexController.ContentInfo(content, 0);

                Console.WriteLine($"{info}");

                return Json(content);
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
        public static string ContentInfo(JObject content, int deep)
        {
            try
            {
                content = content ?? new JObject();

                string info = "";
                foreach (var input in content.Properties())
                {
                    JToken jToken = content[input.Name];
                    switch (jToken.Type)
                    {
                        case JTokenType.Null:
                            info += $"{{{{newline}}}}{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}: null";
                            break;
                        case JTokenType.Object:
                            info += $"{{{{newline}}}}{"".PadLeft((deep + 2) * 4, ' ')}- {input.Name}:";
                            info += IndexController.ContentInfo(jToken.ToObject<JObject>(), deep + 1);
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
