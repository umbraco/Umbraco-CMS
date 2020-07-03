using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // TODO: We need a better name, not sure why the class is called ContentNested in the first place
    public interface IContentNestedDataByteSerializer : IContentNestedDataSerializer
    {
        ContentNestedData DeserializeBytes(byte[] data);
        byte[] SerializeBytes(ContentNestedData nestedData);
    }

    // TODO: We need a better name, not sure why the class is called ContentNested in the first place
    public interface IContentNestedDataSerializer
    {
        ContentNestedData Deserialize(string data);        
        string Serialize(ContentNestedData nestedData);        
    }
}
