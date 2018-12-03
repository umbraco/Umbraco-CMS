using System;

namespace Umbraco.Core.Serialization
{
    public class Formatter : IFormatter
    {
        #region Implementation of IFormatter

        public string Intent
        {
            get { throw new NotImplementedException(); }
        }

        public ISerializer Serializer
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
