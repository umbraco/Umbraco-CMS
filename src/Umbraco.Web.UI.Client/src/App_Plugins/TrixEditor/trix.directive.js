// https://github.com/sachinchoolur/angular-trix
(function() {
    'use strict';
    angular.module('umbraco.directives').directive('trixEditor', function() {
        return {
            restrict: 'E',
            require: 'ngModel',
            scope: {
                /*
                trixInitialize: '&',
                trixChange: '&',
                trixSelectionChange: '&',
                trixFocus: '&',
                trixBlur: '&',
                trixFileAccept: '&',
                trixAttachmentAdd: '&',
                trixAttachmentRemove: '&'
                */
            },
            link: function($scope, element, attr, ngModel, notificationsService, localizationService) {
                
                
                // TODO: retrive from configuration:
                var acceptedFileTypes = ["image/jpeg", "image/png"];
                
                
                var editorElement = element[0];
                
                
                function init() {
                    
                    // if we have some content, apply it to the Trix RTE.
                    if (ngModel.$modelValue) {
                        editorElement.editor.loadHTML(ngModel.$modelValue);
                    }
                    
                    addUploadFeature();
                    
                }
                
                
                editorElement.addEventListener('trix-initialize', init);
                
                // maintenance of ng-model
                
                editorElement.addEventListener('trix-change', function() {
                    ngModel.$setViewValue(element.html());
                });
                
                ngModel.$render = function() {
                    if (editorElement.editor) {
                        editorElement.editor.loadHTML(ngModel.$modelValue);
                    }
                };
                
                
                
                // Apply upload-dialog:
                
                function addUploadFeature() {
                    var toolbar = editorElement.toolbarElement;
                    var ttools  = toolbar.querySelector(".trix-button-group--text-tools");
                    var dialogs = toolbar.querySelector(".trix-dialogs");
                    var trixId  = editorElement.trixId;
                    
                    var buttonContent = `
                      <button type="button"
                        class="trix-button trix-button--icon trix-button--icon-attach"
                        data-trix-attribute="attach"
                        data-trix-key="+" title="Attach file" tabindex="-1">
                      </button>
                    `;

                    var dialogContent = `
                      <div class="trix-dialog trix-dialog--attach" data-trix-dialog="attach" data-trix-dialog-attribute="attach">
                        <div class="trix-dialog__attach-fields">
                          <input type="file" class="trix-input trix-input--dialog">
                          <div class="trix-button-group">
                            <input type="button" class="trix-button trix-button--dialog"
                              onclick="
                                var trix = document.querySelector('trix-editor[trix-id=\\'${trixId}\\']');
                                var fileElm = this.parentElement.parentElement.querySelector('input[type=\\'file\\']');
                                if ( fileElm.files.length == 0 ) {
                                  console.log('nothing selected');
                                  return;
                                }
                                var file = fileElm.files[0];
                                trix.editor.insertFile(file);
                              "
                              value="Attach" data-trix-method="removeAttribute">
                            <input type="button" class="trix-button trix-button--dialog" value="Cancel" data-trix-method="removeAttribute">
                          </div>
                        </div>
                      </div>
                    `;
                    // add attach icon button
                    ttools.insertAdjacentHTML("beforeend", buttonContent);
                    // add dialog
                    dialogs.insertAdjacentHTML("beforeend", dialogContent);
                }
                
                // handling of file uploads
                
                editorElement.addEventListener('trix-file-accept', function(e) {
                    if (acceptedFileTypes.indexOf(e.file.type) !== -1) {
                    } else {
                        e.preventDefault();
                        localizationService.localizeMany([
                                "media_uploadFileDisallowedHeadline",
                                "media_uploadFileDisallowedDescription"
                            ], 
                            [
                                undefined, 
                                [e.file.type]
                            ]
                        ).then(function (data) {
                            notificationsService.error(data[0], data[1]);
                        });
                    }
                });
                
                editorElement.addEventListener('trix-attachment-add', function(event) {
                    
                    var attachment = event.attachment;

                    if (attachment.file) {
                    
                        console.log('trix-attachment-add', attachment.file.name);
                        /*
                        // set caption
                        
                        var editor = editorElement.editor;
                        var originalRange = editor.getSelectedRange()
                        var attachmentRange = editor.getDocument().getRangeOfAttachment(attachment)

                        editor.setSelectedRange(attachmentRange)
                        editor.activateAttribute("caption", "<Insert Caption>")
                        editor.setSelectedRange(originalRange)
                        */
                    }
                    
                });
                
                
                /*
                function registerEvent(type, callbackMethod) {
                    
                    element[0].addEventListener(type, function(e) {
                        
                        if (type === 'trix-file-accept' && attrs.preventTrixFileAccept === 'true') {
                            e.preventDefault();
                        }
                        
                        callbackMethod({
                            e: e,
                            editor: editorElement
                        });
                    });
                    
                };
                
                registerEvent('trix-initialize', $scope.trixInitialize);
                registerEvent('trix-change', $scope.trixChange);
                registerEvent('trix-selection-change', $scope.trixSelectionChange);
                registerEvent('trix-focus', $scope.trixFocus);
                registerEvent('trix-blur', $scope.trixBlur);
                registerEvent('trix-file-accept', $scope.trixFileAccept);
                registerEvent('trix-attachment-add', $scope.trixAttachmentAdd);
                registerEvent('trix-attachment-remove', $scope.trixAttachmentRemove);
                */
            }
        };
    });

}());
