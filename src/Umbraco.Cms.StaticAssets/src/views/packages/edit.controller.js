(function () {
  "use strict";

  function EditController($scope, $location, $routeParams, umbRequestHelper, entityResource, packageResource, editorService, formHelper, localizationService) {

    const vm = this;

    const packageId = $routeParams.id;
    const create = $routeParams.create;

    vm.showBackButton = true;

    // open all expansion panels
    vm.loading = true;
    vm.mediaNodeDisplayModels = [];
    vm.back = back;
    vm.createOrUpdatePackage = createOrUpdatePackage;
    vm.removeContentItem = removeContentItem;
    vm.openContentPicker = openContentPicker;
    vm.openViewPicker = openViewPicker;
    vm.removePackageView = removePackageView;
    vm.downloadFile = downloadFile;

    vm.selectDocumentType = selectDocumentType;
    vm.selectMediaType = selectMediaType;
    vm.selectTemplate = selectTemplate;
    vm.selectStyleSheet = selectStyleSheet;
    vm.selectScript = selectScript;
    vm.selectPartialView = selectPartialView;
    vm.selectMacro = selectMacro;
    vm.selectLanguage = selectLanguage;
    vm.selectDictionaryItem = selectDictionaryItem;
    vm.selectDataType = selectDataType;

    vm.mediaPickerModel = {
      hideLabel: true,
      view: "mediapicker",
      value: "",
      config: {
        multiPicker: true,
        allowEdit: false
      }
    }
    vm.labels = {};

    vm.versionRegex = /^(\d+\.)(\d+\.)(\*|\d+)$/;

    vm.aceOption = {
      mode: "xml",
      theme: "chrome",
      showPrintMargin: false,
      advanced: {
        fontSize: '14px',
        enableSnippets: true,
        enableBasicAutocompletion: true,
        enableLiveAutocompletion: false
      },
      onLoad: function (_editor) {
        vm.editor = _editor;

        vm.editor.setValue(vm.package.actions);
      }
    };

    function onInit() {

      if (create) {
        // Pre populate package with some values
        packageResource.getEmpty().then(scaffold => {
          vm.package = scaffold;

          loadResources();

          vm.loading = false;
        });

        localizationService.localizeMany(["general_create", "packager_includeAllChildNodes"]).then(function (values) {
          vm.labels.button = values[0];
          vm.labels.includeAllChildNodes = values[1];
        });
      } else {
        // Load package
        packageResource.getCreatedById(packageId).then(createdPackage => {
          vm.package = createdPackage;

          loadResources();

          vm.loading = false;

          // Get render model for content node
          if (vm.package.contentNodeId) {
            entityResource.getById(vm.package.contentNodeId, "Document")
              .then((entity) => {
                vm.contentNodeDisplayModel = entity;
              });
          }

          vm.mediaPickerModel.value = vm.package.mediaUdis.join(',');
        });


        localizationService.localizeMany(["buttons_save", "packager_includeAllChildNodes"]).then(function (values) {
          vm.labels.button = values[0];
          vm.labels.includeAllChildNodes = values[1];
        });
      }
    }

    function loadResources() {

      // Get all document types
      entityResource.getAll("DocumentType").then(documentTypes => {
        // a package stores the id as a string so we
        // need to convert all ids to string for comparison
        documentTypes.forEach(documentType => {
          documentType.id = documentType.id.toString();
          documentType.selected = vm.package.documentTypes.indexOf(documentType.id) !== -1;
        });
        vm.documentTypes = documentTypes;
      });

      // Get all media types
      entityResource.getAll("MediaType").then(mediaTypes => {
        // a package stores the id as a string so we
        // need to convert all ids to string for comparison
        mediaTypes.forEach(mediaType => {
          mediaType.id = mediaType.id.toString();
          mediaType.selected = vm.package.mediaTypes.indexOf(mediaType.id) !== -1;
        });
        vm.mediaTypes = mediaTypes;
      });

      // Get all templates
      entityResource.getAll("Template").then(templates => {
        // a package stores the id as a string so we
        // need to convert all ids to string for comparison
        templates.forEach(template => {
          template.id = template.id.toString();
          template.selected = vm.package.templates.indexOf(template.id) >= 0;
        });
        vm.templates = templates;
      });

      // Get all stylesheets
      entityResource.getAll("Stylesheet").then(stylesheets => {
        stylesheets.forEach(stylesheet => {
          stylesheet.selected = vm.package.stylesheets.indexOf(stylesheet.path) >= 0;
        });
        vm.stylesheets = stylesheets;
      });

      // Get all scripts
      entityResource.getAll("Script").then(scripts => {
        scripts.forEach(script => {
          script.selected = vm.package.scripts.indexOf(script.path) >= 0;
        });
        vm.scripts = scripts;
      });

      // Get all partial views
      entityResource.getAll("PartialView").then(partialViews => {
        partialViews.forEach(view => {
          view.selected = vm.package.partialViews.indexOf(view.path) >= 0;
        });
        vm.partialViews = partialViews;
      });

      // Get all macros
      entityResource.getAll("Macro").then(macros => {
        // a package stores the id as a string so we
        // need to convert all ids to string for comparison
        macros.forEach(macro => {
          macro.id = macro.id.toString();
          macro.selected = vm.package.macros.indexOf(macro.id) !== -1;
        });
        vm.macros = macros;
      });

      // Get all languages
      entityResource.getAll("Language").then(languages => {
        // a package stores the id as a string so we
        // need to convert all ids to string for comparison
        languages.forEach(language => {
          language.id = language.id.toString();
          language.selected = vm.package.languages.indexOf(language.id) !== -1;
        });
        vm.languages = languages;
      });

      // Get all dictionary items
      entityResource.getAll("DictionaryItem").then(dictionaryItems => {
        // a package stores the id as a string so we
        // need to convert all ids to string for comparison
        dictionaryItems.forEach(dictionaryItem => {
          dictionaryItem.id = dictionaryItem.id.toString();
          dictionaryItem.selected = vm.package.dictionaryItems.indexOf(dictionaryItem.id) !== -1;
        });
        vm.dictionaryItems = dictionaryItems;
      });

      // Get all data types
      entityResource.getAll("DataType").then(dataTypes => {
        // a package stores the id as a string so we
        // need to convert all ids to string for comparison
        dataTypes.forEach(dataType => {
          dataType.id = dataType.id.toString();
          dataType.selected = vm.package.dataTypes.indexOf(dataType.id) !== -1;
        });
        vm.dataTypes = dataTypes;
      });

    }

    function downloadFile(id) {
      var url = umbRequestHelper.getApiUrl(
        "packageApiBaseUrl",
        "DownloadCreatedPackage",
        { id: id });

      umbRequestHelper.downloadFile(url).then(function () {

      });
    }

    function back() {
      $location.path("packages/packages/created").search("create", null).search("packageId", null);
    }

    function createOrUpdatePackage(editPackageForm) {

      // Split by comma and remove empty entries
      vm.package.mediaUdis = vm.mediaPickerModel.value.split(",").filter(i => i);
      if (formHelper.submitForm({ formCtrl: editPackageForm, scope: $scope })) {

        vm.buttonState = "busy";

        packageResource.savePackage(vm.package).then((updatedPackage) => {

          vm.package = updatedPackage;
          vm.buttonState = "success";

          formHelper.resetForm({ scope: $scope, formCtrl: editPackageForm });

          if (create) {
            //if we are creating, then redirect to the correct url and reload
            $location.path("packages/packages/edit/" + vm.package.id).search("create", null);
            //don't add a browser history for this
            $location.replace();
          }

        }, function (err) {
          formHelper.resetForm({ scope: $scope, formCtrl: editPackageForm, hasErrors: true });
          formHelper.handleError(err);
          vm.buttonState = "error";
        });
      }
    }

    function removeContentItem() {
      vm.package.contentNodeId = null;
    }

    function openContentPicker() {
      const contentPicker = {
        submit: function (model) {
          if (model.selection && model.selection.length > 0) {
            vm.package.contentNodeId = model.selection[0].id.toString();
            vm.contentNodeDisplayModel = model.selection[0];
          }
          editorService.close();
        },
        close: function () {
          editorService.close();
        }
      };
      editorService.contentPicker(contentPicker);
    }

    function openViewPicker() {
        
        const controlPicker = {
          title: "Select view",
          onlyInitialized: false,
          filter: i => {
            if (i.name.indexOf(".html") === -1 && i.name.indexOf(".htm") === -1) {
              return true;
            }
          },
          filterCssClass: "not-allowed",
          select: node => {
              const id = decodeURIComponent(node.id.replace(/\+/g, " "));
              vm.package.packageView = id;
              editorService.close();
          },
          close: () => editorService.close()
        };

        editorService.filePicker(controlPicker);
    }

    function removePackageView() {
      vm.package.packageView = null;
    }

    function selectDocumentType(doctype) {

      // Check if the document type is already selected.
      var index = vm.package.documentTypes.indexOf(doctype.id);

      if (index === -1) {
        vm.package.documentTypes.push(doctype.id);
      } else {
        vm.package.documentTypes.splice(index, 1);
      }
    }

    function selectMediaType(mediatype) {

      // Check if the document type is already selected.
      var index = vm.package.mediaTypes.indexOf(mediatype.id);

      if (index === -1) {
        vm.package.mediaTypes.push(mediatype.id);
      } else {
        vm.package.mediaTypes.splice(index, 1);
      }
    }

    function selectTemplate(template) {

      // Check if the template is already selected.
      var index = vm.package.templates.indexOf(template.id);

      if (index === -1) {
        vm.package.templates.push(template.id);
      } else {
        vm.package.templates.splice(index, 1);
      }
    }

    function selectStyleSheet(stylesheet) {

      // Check if the style sheet is already selected.
      var index = vm.package.stylesheets.indexOf(stylesheet.path);

      if (index === -1) {
        vm.package.stylesheets.push(stylesheet.path);
      } else {
        vm.package.stylesheets.splice(index, 1);
      }
    }

    function selectScript(script) {

      // Check if the script is already selected.
      var index = vm.package.scripts.indexOf(script.path);

      if (index === -1) {
        vm.package.scripts.push(script.path);
      } else {
        vm.package.scripts.splice(index, 1);
      }
    }

    function selectPartialView(view) {

      // Check if the view is already selected.
      var index = vm.package.partialViews.indexOf(view.path);

      if (index === -1) {
        vm.package.partialViews.push(view.path);
      } else {
        vm.package.partialViews.splice(index, 1);
      }
    }

    function selectMacro(macro) {

      // Check if the macro is already selected.
      var index = vm.package.macros.indexOf(macro.id);

      if (index === -1) {
        vm.package.macros.push(macro.id);
      } else {
        vm.package.macros.splice(index, 1);
      }
    }

    function selectLanguage(language) {

      // Check if the language is already selected.
      var index = vm.package.languages.indexOf(language.id);

      if (index === -1) {
        vm.package.languages.push(language.id);
      } else {
        vm.package.languages.splice(index, 1);
      }
    }

    function selectDictionaryItem(dictionaryItem) {

      // Check if the dictionary item is already selected.
      var index = vm.package.dictionaryItems.indexOf(dictionaryItem.id);

      if (index === -1) {
        vm.package.dictionaryItems.push(dictionaryItem.id);
      } else {
        vm.package.dictionaryItems.splice(index, 1);
      }
    }

    function selectDataType(dataType) {

      // Check if the dictionary item is already selected.
      var index = vm.package.dataTypes.indexOf(dataType.id);

      if (index === -1) {
        vm.package.dataTypes.push(dataType.id);
      } else {
        vm.package.dataTypes.splice(index, 1);
      }
    }

    function getVals(array) {
      var vals = [];
      for (var i = 0; i < array.length; i++) {
        vals.push({ value: array[i] });
      }
      return vals;
    }

    onInit();

  }

  angular.module("umbraco").controller("Umbraco.Editors.Packages.EditController", EditController);

})();
