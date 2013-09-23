/**
 * @ngdoc service
 * @name umbraco.services.tinyMceService
 *
 *  
 * @description
 * A service containing all logic for all of the Umbraco TinyMCE plugins
 */
function tinyMceService(dialogService, $log, imageHelper, assetsService, $timeout) {
    return {
        
        /**
        * @ngdoc method
        * @name umbraco.services.tinyMceService#createInsertEmbeddedMedia
        * @methodOf umbraco.services.tinyMceService
        *
        * @description
        * Creates the umbrco insert embedded media tinymce plugin
        *
        * @param {Object} editor the TinyMCE editor instance        
        * @param {Object} $scope the current controller scope
        */
        createInsertEmbeddedMedia: function (editor, $scope) {
            editor.addButton('umbembeddialog', {
                icon: 'media',
                tooltip: 'Embed',
                onclick: function () {
                    dialogService.embedDialog({
                        scope: $scope, callback: function (data) {
                            editor.insertContent(data);
                        }
                    });
                }
            });
        },

        /**
        * @ngdoc method
        * @name umbraco.services.tinyMceService#createMediaPicker
        * @methodOf umbraco.services.tinyMceService
        *
        * @description
        * Creates the umbrco insert media tinymce plugin
        *
        * @param {Object} editor the TinyMCE editor instance        
        * @param {Object} $scope the current controller scope
        */
        createMediaPicker: function(editor, $scope) {
            editor.addButton('umbmediapicker', {
                icon: 'media',
                tooltip: 'Media Picker',
                onclick: function () {
                    dialogService.mediaPicker({
                        scope: $scope, callback: function (img) {

                            if (img) {
                                var imagePropVal = imageHelper.getImagePropertyValue({ imageModel: img, scope: $scope });
                                var data = {
                                    src: (imagePropVal != null && imagePropVal != "") ? imagePropVal : "nothing.jpg",
                                    id: '__mcenew'
                                };


                                editor.insertContent(editor.dom.createHTML('img', data));

                                $timeout(function () {
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
                        }
                    });
                }
            });
        },
        
        /**
        * @ngdoc method
        * @name umbraco.services.tinyMceService#createIconPicker
        * @methodOf umbraco.services.tinyMceService
        *
        * @description
        * Creates the umbrco insert icon tinymce plugin
        *
        * @param {Object} editor the TinyMCE editor instance    
        * @param {Object} $scope the current controller scope
        */
        createIconPicker: function (editor, $scope) {
            editor.addButton('umbiconpicker', {
                icon: 'media',
                tooltip: 'Icon Picker',
                onclick: function () {
                    dialogService.open({
                        show: true, template: "views/common/dialogs/iconpicker.html", scope: $scope, callback: function (c) {

                            var data = {
                                style: 'font-size: 60px'
                            };

                            var i = editor.dom.createHTML('i', data);
                            tinyMCE.activeEditor.dom.addClass(i, c);
                            editor.insertContent(i);
                        }
                    });
                }
            });
        },

        /**
        * @ngdoc method
        * @name umbraco.services.tinyMceService#createUmbracoMacro
        * @methodOf umbraco.services.tinyMceService
        *
        * @description
        * Creates the insert umbrco macro tinymce plugin
        *
        * @param {Object} editor the TinyMCE editor instance      
        * @param {Object} $scope the current controller scope
        */
        createInsertMacro: function (editor, $scope) {
            editor.addButton('umbmacro', {
                icon: 'custom icon-settings-alt',
                tooltip: 'Insert macro',
                onclick: function () {

                    dialogService.open({
                        show: true, template: "views/common/dialogs/insertmacro.html", scope: $scope, callback: function (c) {

                            var data = {
                                style: 'font-size: 60px'
                            };

                            var i = editor.dom.createHTML('i', data);
                            tinyMCE.activeEditor.dom.addClass(i, c);
                            editor.insertContent(i);
                        }
                    });

                }
            });
        }
    };
}

angular.module('umbraco.services').factory('tinyMceService', tinyMceService);