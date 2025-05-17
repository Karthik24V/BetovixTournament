using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tournament.Common.Helpers
{
    public static class JsonToObjectDeserializer
    {
        public static T StringToObjectConvertor<T>(string metaData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(metaData, options) ;
        }
    }
}
