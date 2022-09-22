using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Utility
{
    public static class SessionExtension
    {
        //this ISession session=> extensionv metot anlamındadır
        public static void SetObject(this ISession session,string key,object value)
        {
            //JsonConvert yapılmasının sebebi ön yüzde okumasının kolay olabilmesi içindir
            session.SetString(key,JsonConvert.SerializeObject(value));
        }

        public static  T GetObject<T>(this ISession session,string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
