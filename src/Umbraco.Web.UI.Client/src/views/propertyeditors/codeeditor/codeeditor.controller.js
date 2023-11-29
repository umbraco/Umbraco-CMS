function CodeEditorController($scope) {

    const vm = this;

    const config = {
        showGutter: 1,
        useWrapMode: 1,
        showInvisibles: 0,
        showIndentGuides: 0,
        useSoftTabs: 1,
        showPrintMargin: 0,
        disableSearch: 0,
        theme: "chrome",
        mode: "javascript",
        firstLineNumber: 1,
        fontSize: "small",
        enableSnippets: 0,
        enableBasicAutocompletion: 0,
        enableLiveAutocompletion: 0,
        readonly: 0,
        minLines: undefined,
        maxLines: undefined
    };
    
    Utilities.extend(config, $scope.model.config);

    // map back to the model
    $scope.model.config = config;

    function init() {

      vm.readonly = Object.toBoolean(config.readonly);

      //vm.options = {
      //    autoFocus: false,
      //    showGutter: Object.toBoolean(config.showGutter),
      //    useWrapMode: Object.toBoolean(config.useWrapMode),
      //    showInvisibles: Object.toBoolean(config.showInvisibles),
      //    showIndentGuides: Object.toBoolean(config.showIndentGuides),
      //    useSoftTabs: Object.toBoolean(config.useSoftTabs),
      //    showPrintMargin: Object.toBoolean(config.showPrintMargin),
      //    disableSearch: Object.toBoolean(config.disableSearch),
      //    theme: config.theme,
      //    mode: config.mode,
      //    firstLineNumber: config.firstLineNumber,
      //    advanced: {
      //        fontSize: config.fontSize,
      //        enableSnippets: Object.toBoolean(config.enableSnippets),
      //        enableBasicAutocompletion: Object.toBoolean(config.enableBasicAutocompletion),
      //        enableLiveAutocompletion: Object.toBoolean(config.enableLiveAutocompletion),
      //        minLines: config.minLines,
      //        maxLines: config.maxLines,
      //        wrap: Object.toBoolean(config.useWrapMode)
      //    }
      //};

      vm.aceOption = {
          mode: "razor",
          theme: "chrome",
          showPrintMargin: false,
          autoFocus: false,
          advanced: {
              fontSize: "14px",
              enableSnippets: false,
              enableBasicAutocompletion: true,
              enableLiveAutocompletion: false,
              //minLines: config.minLines,
              //maxLines: config.maxLines,
              wrap: true
          },
          onLoad: function (aceEditor) {
              vm.aceEditor = aceEditor;
          }
      }

    };

    init();

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.CodeEditorController", CodeEditorController);
