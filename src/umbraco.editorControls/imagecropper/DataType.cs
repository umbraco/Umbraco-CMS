using System;
using System.Reflection;

namespace umbraco.editorControls.imagecropper
{
    public class DataType : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
    {
        private umbraco.interfaces.IDataEditor _editor;
        private umbraco.interfaces.IData _baseData;
        private PrevalueEditor _prevalueEditor;

        public override umbraco.interfaces.IDataEditor DataEditor
        {
            get
            {
                if (_editor == null)
                    _editor = new DataEditor(Data, ((PrevalueEditor)PrevalueEditor).Configuration);
                return _editor;
            }
        }

        public override umbraco.interfaces.IData Data
        {
            get
            {
                if (_baseData == null)
                    _baseData = new DataTypeData(this);
                return _baseData;
            }
        }
        public override Guid Id
        {
            get { return new Guid("7A2D436C-34C2-410F-898F-4A23B3D79F54"); }
        }

        public override string DataTypeName
        {
            get { return "Image Cropper"; }
        }        

        public override umbraco.interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueEditor == null)
                    _prevalueEditor = new PrevalueEditor(this);
                return _prevalueEditor;                
            }
        }

        public static int Version
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return version.Major*1000 + version.Minor*100;
            }
        }
    }
}