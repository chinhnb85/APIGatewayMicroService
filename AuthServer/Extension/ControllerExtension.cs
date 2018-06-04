using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Extension
{
    public static class ControllerExtension
    {
        public static JsonResult ToJson<T>(this Controller self, T data)
        {
            if (data == null)
            {
                return self.Json(data);
            }
            if (data is string)
            {
                var temp = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data as string);
                return self.Json(temp, new Newtonsoft.Json.JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"
                });
            }
            return self.Json(data, new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"
            });
        }

        public static T DeJson<T>(this Controller self, string data)
        {
            if (data == null)
            {
                return default(T);
            }
            if (data is string)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
            }
            return default(T);
        }
    }
}
