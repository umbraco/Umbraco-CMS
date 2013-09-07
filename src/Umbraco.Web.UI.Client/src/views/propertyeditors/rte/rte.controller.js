angular.module("umbraco")
    .controller("Umbraco.Editors.RTEController",
    function($rootScope, $scope, dialogService, $log, imageHelper, assetsService, $timeout){

    assetsService.loadJs("lib/tinymce/tinymce.min.js", $scope).then(function(){
        tinymce.DOM.events.domLoaded = true;

        tinymce.init({
            selector: "#" + $scope.model.alias + "_rte",
            skin: "umbraco",
            menubar : false,
            statusbar: false,
            height: 340,
            toolbar: "bold italic | styleselect | alignleft aligncenter alignright | bullist numlist | outdent indent | link image mediapicker iconpicker",
            setup : function(editor) {
                    

                    editor.on('blur', function(e) {
                        $scope.$apply(function() {
                            $scope.model.value = editor.getContent();
                        });
                    });

                    editor.addButton('mediapicker', {
                        icon: 'media',
                        tooltip: 'Media Picker',
                        onclick: function(){
                            dialogService.mediaPicker({scope: $scope, callback: function(img){
                                
                                if(img){
                                    var imagePropVal = imageHelper.getImagePropertyValue({imageModel: img, scope: $scope});
                                    var data = {
                                        src: (imagePropVal != null && imagePropVal != "") ? imagePropVal: "nothing.jpg",
                                        id: '__mcenew'
                                    };
                                    
                                    $log.log(data);

                                    editor.insertContent(editor.dom.createHTML('img', data));
                                    
                                    $timeout(function(){
                                         var imgElm = editor.dom.get('__mcenew');
                                         var size = editor.dom.getSize(imgElm);
                                         $log.log(size);

                                         var newSize = imageHelper.scaleToMaxSize(500, size.w, size.h);
                                         var s = "width: " + newSize.width + "px; height:" + newSize.height + "px;";
                                         editor.dom.setAttrib(imgElm, 'style', s);
                                         editor.dom.setAttrib(imgElm, 'rel', newSize.width + "," + newSize.height);
                                         editor.dom.setAttrib(imgElm, 'id', null);

                                         
                                    }, 500);
                                }
                            }});
                        }
                    });

                    editor.addButton('iconpicker', {
                        icon: 'media',
                        tooltip: 'Icon Picker',
                        onclick: function(){
                            dialogService.open({show: true, template: "views/common/dialogs/iconpicker.html", scope: $scope, callback: function(c){
                               
                                var data = {
                                    style: 'font-size: 60px'
                                };

                                var i = editor.dom.createHTML('i', data);
                                tinyMCE.activeEditor.dom.addClass(i, c);
                                editor.insertContent(i);
                            }});
                        }
                    });    
              }
        });


        $scope.openMediaPicker =function(value){
                var d = dialog.mediaPicker({scope: $scope, callback: populate});
        };

        function populate(data){
            $scope.model.value = data.selection;   
        }

    });
});