using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core;

namespace umbraco.editorControls.relatedlinks
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class RelatedLinksDataType : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
    {
        private umbraco.interfaces.IDataEditor _Editor;
        private umbraco.interfaces.IData _baseData;
        private RelatedLinksPrevalueEditor _prevalueeditor;

        public override umbraco.interfaces.IDataEditor DataEditor
        {
            get
            {

                if (_Editor == null)
                    _Editor = new RelatedLinksDataEditor(Data, ((RelatedLinksPrevalueEditor)PrevalueEditor).Configuration);
                return _Editor;
            }
        }

        public override umbraco.interfaces.IData Data
        {
            get
            {
                if (_baseData == null)
                    _baseData = new RelatedLinksData(this);
                return _baseData;
            }
        }
        public override Guid Id
        {
            get { return new Guid(Constants.PropertyEditors.RelatedLinks); }
        }

        public override string DataTypeName
        {
            get { return "Related Links"; }
        }

        public override umbraco.interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueeditor == null)
                    _prevalueeditor = new RelatedLinksPrevalueEditor(this);
                return _prevalueeditor;
            }
        }
    }
}
