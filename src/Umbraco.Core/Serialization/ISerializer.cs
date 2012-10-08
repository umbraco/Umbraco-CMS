using System;
using System.IO;

namespace Umbraco.Core.Serialization
{
    public interface ISerializer
    {
        object FromStream(Stream input, Type outputType);

        IStreamedResult ToStream(object input);
    }
}