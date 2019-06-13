// https://github.com/sachinchoolur/angular-trix
(function() {
    'use strict';
    angular.module('umbraco.directives').directive('trixEditor', function(Upload, umbRequestHelper, localizationService, notificationsService) {
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
            link: function($scope, element, attr, ngModel) {
                
                
                // TODO: retrive from configuration:
                var acceptedFileTypes = ["image/jpeg", "image/png"];
                var maxFileSize = 37000000;
                
                
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
                    
                    // Prevent attaching files > 1024 bytes
                    //if (event.file.size > 1024) {
                    //  event.preventDefault()
                    //}
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
                        
                        
                        if (attachment.file.size == 0) {
                            attachment.remove();
                            return;
                            // You may personalize max_file_size
                        } else if (attachment.file.size > maxFileSize) {
                            attachment.remove();
                            return;
                        }
                        
                        uploadAttachment(attachment);
                        
                        
                    }
                    
                });
                
                
                
                
                
                var attachmentsQueue = [];
                
                function uploadAttachment(attachment) {
                    attachmentsQueue.push(attachment);
                    uploadNextAttachment();
                }
                function uploadNextAttachment() {
                    if (attachmentsQueue.length > 0) {
                        uploadAttachmentToMedia(attachmentsQueue.shift());
                    }
                }
                
                function uploadAttachmentToMedia(attachment) {
                    
                    var file = attachment.file;
                    
                    //attachment.attachment.attributes = attachment.attachment.attributes.add("data-udi", ".................");
                    
                    Upload.upload({
                            url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
                            fields: {
                                'currentFolder': -1,
                                'contentTypeAlias': "umbracoAutoSelect",
                                'propertyAlias': "umbracoFile",
                                'path': file.path
                            },
                            file: file
                        })
                        .progress(function(evt) {
                            //if (file.uploadStat !== "done" && file.uploadStat !== "error") {
                              // calculate progress in percentage
                              var progressPercentage = parseInt(100.0 * evt.loaded / evt.total, 10);
                              // set percentage property on file
                              //file.uploadProgress = progressPercentage;
                              // set uploading status on file
                              //file.uploadStatus = "uploading"; 
                              
                              console.log("progressPercentage:", progressPercentage);
                              attachment.setUploadProgress(progressPercentage);
                            //}
                        })
                        .success(function(data, status, headers, config) {
                            if (data.notifications && data.notifications.length > 0) {
                                // set error status on file
                                //file.uploadStatus = "error";
                                // Throw message back to user with the cause of the error
                                //file.serverErrorMessage = data.notifications[0].message;
                                // Put the file in the rejected pool
                                //scope.rejected.push(file);
                            } else {
                                // set done status on file
                                //file.uploadStatus = "done";
                                //file.uploadProgress = 100;
                                // set date/time for when done - used for sorting
                                //file.doneDate = new Date();
                                // Put the file in the done pool
                                //scope.done.push(file);
                                
                                attachment.setUploadProgress(100);
                                
                                
                                
                                console.log("File uploaded, data: ", data);
                                
                                var file = data.uploadedfiles[0];
                                if(file) {
                                    
                                    // set UDI.. so far it has only been posible for me to set this attribute, which becomes part of the data-trix-attachment attribute on the figure element.
                                    attachment.setAttributes({"udi": file.Id});
                                    
                                    // set the url, Trix will parse on this variable to the img element as src attribute.
                                    attachment.setAttributes({"url": file.TempFilePath});
                                    
                                } else {
                                    // something failed, so we will remove the file again.
                                    attachment.remove();
                                }
                                
                            }
                            
                            //after processing, test if everthing is done
                            uploadNextAttachment();
                        })
                        .error(function(evt, status, headers, config) {
                            // set status done
                            file.uploadStatus = "error";
                            //if the service returns a detailed error
                            if (evt.InnerException) {
                                file.serverErrorMessage = evt.InnerException.ExceptionMessage;
                                //Check if its the common "too large file" exception
                                if (evt.InnerException.StackTrace &&
                                    evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
                                    file.serverErrorMessage = "File too large to upload";
                                }
                            } else if (evt.Message) {
                                file.serverErrorMessage = evt.Message;
                            }
                            // If file not found, server will return a 404 and display this message
                            if (status === 404) {
                                file.serverErrorMessage = "File not found";
                            }
                            //after processing, test if everthing is done
                            //scope.rejected.push(file);
                            
                            attachment.remove();
                            uploadNextAttachment();
                        });
                }
                
                
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
