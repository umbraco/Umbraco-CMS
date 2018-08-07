(function () {
    'use strict';

    /**
     * @ngdoc controller
     * @name Umbraco.Editors.FileUploadController
     * @function
     *
     * @description
     * The controller for the file upload property editor.
     *
    */
    function fileUploadController($scope, $element, $compile, imageHelper, fileManager, umbRequestHelper, mediaHelper, angularHelper) {
        
        /* It is important to note that the $scope.model.value
         *  doesn't necessarily depict what is saved for this property editor. $scope.model.value can be empty when we
         *  are submitting files because in that case, we are adding files to the fileManager which is what gets peristed
         *  on the server. However, when we are clearing files, we are setting $scope.model.value to "{clearFiles: true}"
         *  to indicate on the server that we are removing files for this property. We will keep the $scope.model.value to
         *  be the name of the file selected (if it is a newly selected file) or keep it to be it's original value, this allows
         *  for the editors to check if the value has changed and to re-bind the property if that is true.
         * */

        var vm = this;

        vm.$onInit = onInit;
        vm.$onChanges = onChanges;
        vm.$postLink = postLink;
        vm.validateMandatory = validateMandatory;

        /** Clears the file collections when content is saving (if we need to clear) or after saved */
        function clearFiles() {
            //clear the files collection (we don't want to upload any!)
            fileManager.setFiles({
                propertyAlias: vm.propertyAlias,
                culture: vm.culture,
                files: []
            });
            //clear the current files
            vm.files = [];

            if (vm.fileUploadForm) {
                if (vm.fileUploadForm.fileCount) {
                    angularHelper.revalidateNgModel($scope, vm.fileUploadForm.fileCount);
                }
            }
        }

        /** Called when the component initializes */
        function onInit() {

            //check if there's a tabbed-content controller available and get the culture
            vm.culture = (vm.tabbedContentCtrl && vm.tabbedContentCtrl.content.language) ? vm.tabbedContentCtrl.content.language.culture : null;

            $scope.$watch(function () {
                return vm.clearFiles;
            }, clearFilesWatch);

            $scope.$on("filesSelected", onFilesSelected);
        }

        /** Called when the component has linked all elements, this is when the form controller is available */
        function postLink() {
            initialize();
        }

        /** this method is used to initialize the data and to re-initialize it if the server value is changed */
        function initialize(index) {

            clearFiles();

            if (!index) {
                index = 1;
            }

            //this is used in order to tell the umb-single-file-upload directive to
            //rebuild the html input control (and thus clearing the selected file) since
            //that is the only way to manipulate the html for the file input control.
            vm.rebuildInput = {
                index: index
            };
            //clear the current files
            vm.files = [];
            //store the original value so we can restore it if the user clears and then cancels clearing.
            vm.originalValue = vm.modelValue;

            //create the property to show the list of files currently saved
            if (vm.modelValue != "" && vm.modelValue != undefined) {

                var images = vm.modelValue.split(",");

                vm.persistedFiles = _.map(images, function (item) {
                    return { file: item, isImage: imageHelper.detectIfImageByExtension(item) };
                });
            }
            else {
                vm.persistedFiles = [];
            }

            _.each(vm.persistedFiles, function (file) {

                var thumbnailUrl = umbRequestHelper.getApiUrl(
                    "imagesApiBaseUrl",
                    "GetBigThumbnail",
                    [{ originalImagePath: file.file }]);

                var extension = file.file.substring(file.file.lastIndexOf(".") + 1, file.file.length);

                file.thumbnail = thumbnailUrl + '&rnd=' + Math.random();
                file.extension = extension.toLowerCase();
            });

            vm.clearFiles = false;
        }

        /** Method required by the valPropertyValidator directive (returns true if the property editor has at least one file selected) */
        function validateMandatory() {
            return {
                isValid: !vm.validation.mandatory || (((vm.persistedFiles != null && vm.persistedFiles.length > 0) || (vm.files != null && vm.files.length > 0)) && !vm.clearFiles),
                errorMsg: "Value cannot be empty",
                errorKey: "required"
            };
        }

        /**
         * listen for clear files changes to set our model to be sent up to the server
         * @param {any} newVal
         * @param {any} oldVal
         */
        function clearFilesWatch(newVal, oldVal) {
            //TODO: Call a callback method instead of setting the value directly inside this component

            if (newVal === true) {
                vm.modelValue = { clearFiles: true };
                clearFiles();
            }
            else if (newVal !== oldVal) {
                //reset to original value
                vm.modelValue = vm.originalValue;
                //this is required to re-validate
                if (vm.fileUploadForm) {
                    angularHelper.revalidateNgModel($scope, vm.fileUploadForm.fileCount);
                }
            }
        }

        /**
         * Watch for model changes
         * @param {any} changes
         */
        function onChanges(changes) {

            if (changes.modelValue && !changes.modelValue.isFirstChange()
                && changes.modelValue.currentValue !== null && changes.modelValue.currentValue !== undefined
                && changes.modelValue.currentValue !== changes.modelValue.previousValue) {

                // here we need to check if the value change needs to trigger an update in the UI.
                // if the value is only changed in the controller and not in the server values, we do not
                // want to trigger an update yet.
                // we can however no longer rely on checking values in the controller vs. values from the server
                // to determine whether to update or not, since you could potentially be uploading a file with
                // the exact same name - in that case we need to reinitialize to show the newly uploaded file.
                if (changes.modelValue.currentValue.clearFiles !== true && !changes.modelValue.currentValue.selectedFiles) {
                    initialize(vm.rebuildInput.index + 1);
                }
            }
        }

        /**
         * listen for when a file is selected
         * @param {any} event
         * @param {any} args
         */
        function onFilesSelected(event, args) {
            //set the files collection
            fileManager.setFiles({
                propertyAlias: vm.propertyAlias,
                files: args.files,
                culture: vm.culture
            });
            //clear the current files
            vm.files = [];
            var newVal = "";
            for (var i = 0; i < args.files.length; i++) {
                //save the file object to the files collection
                vm.files.push({ alias: vm.propertyAlias, file: args.files[i] });
                newVal += args.files[i].name + ",";
            }

            //this is required to re-validate
            angularHelper.revalidateNgModel($scope, vm.fileUploadForm.fileCount);

            //set clear files to false, this will reset the model too
            vm.clearFiles = false;

            //TODO: Call a callback method instead of setting the value directly inside this component

            //set the model value to be the concatenation of files selected. Please see the notes
            // in the description of this controller, it states that this value isn't actually used for persistence,
            // but we need to set it so that the editor and the server can detect that it's been changed, and it is used for validation.
            vm.modelValue = { selectedFiles: newVal.trimEnd(",") };

            //need to explicity setDirty here as file upload field can't track dirty & we can't use the fileCount (hidden field/model)
            vm.fileUploadForm.$setDirty();
        }

    };


    var umbFileUploadEditorComponent = {
        templateUrl: 'views/propertyeditors/fileupload/umbfileuploadeditor.component.html',
        require: {
            tabbedContentCtrl: '?^^tabbedContent'
        },
        bindings: {
            modelValue: "=",
            propertyAlias: "@",
            validation: "<"
        },
        controllerAs: 'vm',
        controller: fileUploadController
    };

    angular.module("umbraco")
        //.controller('Umbraco.PropertyEditors.FileUploadController', fileUploadController)
        .component('umbFileUploadEditor', umbFileUploadEditorComponent)
        .run(function (mediaHelper, umbRequestHelper, assetsService) {
            if (mediaHelper && mediaHelper.registerFileResolver) {

                //NOTE: The 'entity' can be either a normal media entity or an "entity" returned from the entityResource
                // they contain different data structures so if we need to query against it we need to be aware of this.
                mediaHelper.registerFileResolver("Umbraco.UploadField", function (property, entity, thumbnail) {
                    if (thumbnail) {
                        if (mediaHelper.detectIfImageByExtension(property.value)) {
                            //get default big thumbnail from image processor
                            var thumbnailUrl = property.value + "?rnd=" + moment(entity.updateDate).format("YYYYMMDDHHmmss") + "&width=500&animationprocessmode=first";
                            return thumbnailUrl;
                        }
                        else {
                            return null;
                        }
                    }
                    else {
                        return property.value;
                    }
                });

            }
        });


})();
