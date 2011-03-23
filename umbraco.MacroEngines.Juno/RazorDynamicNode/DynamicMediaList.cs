using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Collections;
using umbraco.cms.businesslogic.media;

namespace umbraco.MacroEngines
{
    public class DynamicMediaList : DynamicObject, IEnumerable
    {
        public IEnumerable<DynamicMedia> Items { get; set; }

        public DynamicMediaList()
        {
            Items = new List<DynamicMedia>();
        }
        public DynamicMediaList(IEnumerable<DynamicMedia> items)
        {
            List<DynamicMedia> list = items.ToList();
            list.ForEach(node => node.ownerList = this);
            Items = list;
        }
        public DynamicMediaList(IEnumerable<Media> items)
        {
            List<DynamicMedia> list = items.Select(x => new DynamicMedia(x)).ToList();
            list.ForEach(node => node.ownerList = this);
            Items = list;
        }

        public IEnumerator GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public bool IsNull()
        {
            return false;
        }
        public bool HasValue()
        {
            return true;
        }
    }
}
