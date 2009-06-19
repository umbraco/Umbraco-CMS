namespace umbraco.uicontrols {
    public class MenuIconClass : MenuIconI {

        private string _id;
        private string _imageURL;
        private string _onClickCommand;
        private string _AltText;
        private int _width;
        private int _height;


        public string ID {
            get { return _id; }
            set { _id = value; }
        }

        public string AltText {
            get { return _AltText; }
            set { _AltText = value; }
        }
        public int IconWidth {
            get { return _width; }
            set { _width = value; }
        }
        public int IconHeight {
            get { return _height; }
            set { _height = value; }
        }
        public string ImageURL {
            get { return _imageURL; }
            set { _imageURL = value; }
        }

        public string OnClickCommand {
            get { return _onClickCommand; }
            set { _onClickCommand = value; }
        }
    }
}