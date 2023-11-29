using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SnowFall.Helper;

public class JsonHelper
{
    public static void JsonArrayToList(ICollection<int> collection, string key, JsonObject obj, string? exceptionMeessage = null)
    {
        if (obj != null)
        {
            if (obj.ContainsKey(key))
            {
                foreach (var elem in obj[key] as JsonArray ?? throw new Exception(exceptionMeessage))
                {
                    var item = (int)(elem ?? throw new Exception("slot info symbol array quick pay is null"));
                    collection.Add(item);
                }
            }
        }
    }
}
