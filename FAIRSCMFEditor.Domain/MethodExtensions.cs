using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public static class MethodExtensions
    {
        /// <summary>
        /// Returns JSON serialization of the object.
        /// </summary>
        /// <param name="value">Object to serialize.</param>
        /// <returns>Json serilization.</returns>
        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<T> ToObjects<T>(this string value)
        {
            List<T> retval = null;
            if (String.IsNullOrEmpty(value))
            {
                retval = new List<T>();
            }
            else
            {
                retval = JsonConvert.DeserializeObject<List<T>>(value);
            }
            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToObject<T>(this string value)
        {
            T retval = default(T);
            if (!String.IsNullOrEmpty(value))
            {
                retval = JsonConvert.DeserializeObject<T>(value);
            }
            return retval;
        }
    }
}
