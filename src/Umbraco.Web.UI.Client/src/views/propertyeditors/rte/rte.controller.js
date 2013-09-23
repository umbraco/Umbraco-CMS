angular.module("umbraco")
    .controller("Umbraco.Editors.RTEController",
    function ($rootScope, $scope, dialogService, $log, imageHelper, assetsService, $timeout, tinyMceService) {

        //TODO: This should be configurable (i.e. from the config file we have and/or from pre-values)
        var validElements = "@[id|class|style|title|dir<ltr?rtl|lang|xml::lang|onclick|ondblclick|"
            + "onmousedown|onmouseup|onmouseover|onmousemove|onmouseout|onkeypress|"
            + "onkeydown|onkeyup],a[rel|rev|charset|hreflang|tabindex|accesskey|type|"
            + "name|href|target|title|class|onfocus|onblur],strong/b,em/i,strike,u,"
            + "#p,-ol[type|compact],-ul[type|compact],-li,br,img[longdesc|usemap|"
            + "src|border|alt=|title|hspace|vspace|width|height|align],-sub,-sup,"
            + "-blockquote,-table[border=0|cellspacing|cellpadding|width|frame|rules|"
            + "height|align|summary|bgcolor|background|bordercolor],-tr[rowspan|width|"
            + "height|align|valign|bgcolor|background|bordercolor],tbody,thead,tfoot,"
            + "#td[colspan|rowspan|width|height|align|valign|bgcolor|background|bordercolor"
            + "|scope],#th[colspan|rowspan|width|height|align|valign|scope],caption,-div,"
            + "-span,-code,-pre,address,-h1,-h2,-h3,-h4,-h5,-h6,hr[size|noshade],-font[face"
            + "|size|color],dd,dl,dt,cite,abbr,acronym,del[datetime|cite],ins[datetime|cite],"
            + "object[classid|width|height|codebase|*],param[name|value|_value],embed[type|width"
            + "|height|src|*],script[src|type],map[name],area[shape|coords|href|alt|target],bdo,"
            + "button,col[align|char|charoff|span|valign|width],colgroup[align|char|charoff|span|"
            + "valign|width],dfn,fieldset,form[action|accept|accept-charset|enctype|method],"
            + "input[accept|alt|checked|disabled|maxlength|name|readonly|size|src|type|value],"
            + "kbd,label[for],legend,noscript,optgroup[label|disabled],option[disabled|label|selected|value],"
            + "q[cite],samp,select[disabled|multiple|name|size],small,"
            + "textarea[cols|rows|disabled|name|readonly],tt,var,big";

        //TODO: This should be configurable (i.e. from the config file we have and/or from pre-values)
        var toolbar = "code | bold italic | styleselect | alignleft aligncenter alignright | bullist numlist | outdent indent | link image umbmediapicker umbiconpicker umbembeddialog umbmacro";

        //TODO: This should be configurable (i.e. from the config file we have and/or from pre-values)
        var plugins = "code";

        assetsService.loadJs("lib/tinymce/tinymce.min.js", $scope).then(function () {
            //we need to add a timeout here, to force a redraw so TinyMCE can find
            //the elements needed
            $timeout(function () {
                tinymce.DOM.events.domLoaded = true;
                tinymce.init({
                    mode: "exact",
                    elements: $scope.model.alias + "_rte",
                    skin: "umbraco",                    
                    plugins: plugins,
                    valid_elements: validElements,
                    menubar: false,
                    statusbar: false,
                    height: 340,
                    toolbar: toolbar,
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