angular.module("umbraco")
    .controller("Umbraco.Editors.RTEController",
    function ($rootScope, $scope, dialogService, $log, imageHelper, assetsService, $timeout, tinyMceService) {

        assetsService.loadJs("lib/tinymce/tinymce.min.js", $scope).then(function () {
            //we need to add a timeout here, to force a redraw so TinyMCE can find
            //the elements needed
            $timeout(function () {
                tinymce.DOM.events.domLoaded = true;
                tinymce.init({
                    mode: "exact",
                    elements: $scope.model.alias + "_rte",
                    skin: "umbraco",
                    //plugins: "pagebreak,style,layer,table,advhr,advimage,advlink,emotions,iespell,inlinepopups,insertdatetime,preview,media,searchreplace,print,paste,directionality,fullscreen,noneditable,visualchars,nonbreaking,xhtmlxtras,advlist,umbracolink,umbracoanchor,umbracostyles,umbracocharmap,umbracomacro,umbracosave,umbracomedia",
                    //plugins: "umbmacro",
                    menubar: false,
                    statusbar: false,
                    height: 340,
                    toolbar: "bold italic | styleselect | alignleft aligncenter alignright | bullist numlist | outdent indent | link image umbmediapicker umbiconpicker umbembeddialog umbmacro",
                    setup: function (editor) {
                        editor.on('blur', function (e) {
                            $scope.$apply(function () {
                                $scope.model.value = editor.getContent();
                            });
                        });

                        //Create the insert media plugin
                        tinyMceService.createMediaPicker(editor, $scope);

                        //Create the insert icon plugin
                        tinyMceService.createIconPicker(editor, $scope);
                        
                        //Create the insert icon plugin
                        tinyMceService.createInsertEmbeddedMedia(editor, $scope);

                        //Create the insert macro plugin
                        tinyMceService.createInsertMacro(editor, $scope);


                    }
                });
            }, 1);

        });
    });