using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence
{

    //TODO: Would be good to use this exception type anytime we cannot find an entity

    /// <summary>
    /// An exception used to indicate that an umbraco entity could not be found
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        public object Id { get; private set; }
        private readonly string _msg;

        public EntityNotFoundException(object id, string msg)
        {
            Id = id;
            _msg = msg;
        }

        public EntityNotFoundException(string msg)
        {
            _msg = msg;
        }

        public override string Message
        {
            get { return _msg; }
        }

        public override string ToString()
        {
            var result = base.ToString();

            if (Id != null)
            {
                return "Umbraco entity (id: " + Id + ") not found. " + result;    
            }

            return result;
        }
    }
}
