angular.module("umbraco")
    .controller("Umbraco.Editors.RTEController", 
    function($rootScope, $scope, dialogService, $log){
    require(
        [
            'tinymce'
        ],
        function (tinymce) {

            tinymce.DOM.events.domLoaded = true;
            tinymce.init({
                selector: "#" + $scope.model.alias + "_rte",
                skin: "umbraco",
                menubar : false,
                statusbar: false,
                height: 340,
                toolbar: "bold italic | styleselect | alignleft aligncenter alignright | bullist numlist | outdent indent | link image mediapicker",
                setup : function(editor) {
                        
                        editor.on('blur', function(e) {
                            $scope.$apply(function() {
                                //$scope.model.value = e.getBody().innerHTML;
                                $scope.model.value = editor.getContent();
                            });
                        });

                        editor.addButton('mediapicker', {
                            icon: 'media',
                            tooltip: 'Media Picker',
                            onclick: function(){
                                dialogService.mediaPicker({scope: $scope, callback: function(data){
                                 
                                    //really simple example on how to intergrate a service with tinyMCE
                                    $(data.selection).each(function (i, img) {

                                        var imageProperty = _.find(img.properties, function(item) {
                                            return item.alias == 'umbracoFile';
                                        });

                                        var data = {
                                            src: imageProperty != null ? imageProperty.value : "nothing.jpg",
                                            style: 'width: 100px; height: 100px',
                                            id: '__mcenew'
                                        };
                                            
                                            editor.insertContent(editor.dom.createHTML('img', data));
                                            var imgElm = editor.dom.get('__mcenew');
                                            editor.dom.setAttrib(imgElm, 'id', null);
                                    });    
                                       

                                }});
                            }
                        });

            
                  }
            });


            $scope.openMediaPicker =function(value){
                    var d = dialog.mediaPicker({scope: $scope, callback: populate});
            };

            function bindValue(inst){
                $log.log("woot");

                $scope.$apply(function() {
                    $scope.model.value = inst.getBody().innerHTML;
                });
            }

            function myHandleEvent(e){
                $log.log(e);
            }

            function populate(data){
                $scope.model.value = data.selection;    
            }

        });
});