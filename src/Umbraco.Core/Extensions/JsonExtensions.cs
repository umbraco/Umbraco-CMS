using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Extensions
{
    public static class JsonExtensions
    {
        public static bool TryParse(string json, out JsonNode? node)
        {
            try
            {
                node = JsonNode.Parse(json);
                return true;
            }
            catch (Exception)
            {
                node = null;
                return false;
            }
        }

        public static bool TryParse(string json, out JsonArray? array)
        {
            try
            {
                array = JsonArray.Parse(json)!.AsArray();
                return true;
            }
            catch (Exception)
            {
                array = null;
                return false;
            }
        }

        public static bool TryParse(string json, out JsonObject? obj)
        {
            try
            {
                obj = JsonNode.Parse(json)!.AsObject();
                return true;
            }
            catch (Exception)
            {
                obj = null;
                return false;
            }
        }
    }
}
