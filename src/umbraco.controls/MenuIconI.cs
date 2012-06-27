namespace umbraco.uicontrols {
    public interface MenuIconI {
        string ImageURL {
            get;
            set;
        }
        string ID {
            get;
            set;
        }
        string OnClickCommand {
            get;
            set;
        }
        string AltText {
            get;
            set;
        }
        int IconWidth {
            get;
            set;
        }
        int IconHeight {
            get;
            set;
        }
    }
}
