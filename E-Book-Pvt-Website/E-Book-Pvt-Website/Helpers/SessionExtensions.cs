using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text.Json;

namespace E_Book_Pvt_Website.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var json = JsonConvert.SerializeObject(value);
            session.SetString(key, json);
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return json == null ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }
    }
}
