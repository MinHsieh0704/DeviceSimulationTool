using Min_Helpers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeviceSimulationTool.Helpers
{
    public class Community
    {
        /// <summary>
        /// Convert Data Validate Exception to Message
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string ConvertDataValidateExceptionToMessage(Exception exception)
        {
            try
            {
                string message = "";

                exception = ExceptionHelper.GetReal(exception);
                if (exception.Data == null)
                {
                    return "";
                }
                if (exception.Data.Values == null)
                {
                    return "";
                }

                foreach (var value in exception.Data.Values)
                {
                    var errorMessages = value as List<string>;
                    if (errorMessages == null)
                    {
                        continue;
                    }

                    message += String.Join("\n", errorMessages);
                    message += "\n";
                }

                message = Regex.Replace(message, "\n$", "");

                return message;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
