(function () {
  "use strict";

  function partialViewMacrosEditController($scope, $routeParams, codefileResource, assetsService, notificationsService, editorState, navigationService, appState, macroService, angularHelper, $timeout, contentEditingHelper, localizationService, templateHelper, macroResource, editorService) {

    var vm = this;

    vm.runtimeModeProduction = Umbraco.Sys.ServerVariables.application.runtimeMode == 'Production';

    vm.header = {};
    vm.header.editorfor = "visuallyHiddenTexts_newPartialViewMacro";
    vm.header.setPageTitle = true;
    vm.page = {};
    vm.page.loading = true;
    vm.partialViewMacroFile = {};

    //menu
    vm.page.menu = {};
    vm.page.menu.currentSection = appState.getSectionState("currentSection");
    vm.page.menu.currentNode = null;

    // insert buttons
    vm.page.insertDefaultButton = {
      labelKey: "general_insert",
      addEllipsis: "true",
      handler: function () {
        vm.openInsertOverlay();
      }
    };
    vm.page.insertSubButtons = [
      {
        labelKey: "template_insertPageField",
        addEllipsis: "true",
        handler: function () {
          vm.openPageFieldOverlay();
        }
      },
      {
        labelKey: "template_insertMacro",
        addEllipsis: "true",
        handler: function () {
          vm.openMacroOverlay()
        }
      },
      {
        labelKey: "template_insertDictionaryItem",
        addEllipsis: "true",
        handler: function () {
          vm.openDictionaryItemOverlay();
        }
      }
    ];

    // bind functions to view model
    vm.save = save;
    vm.openPageFieldOverlay = openPageFieldOverlay;
    vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
    vm.openQueryBuilderOverlay = openQueryBuilderOverlay;
    vm.openMacroOverlay = openMacroOverlay;
    vm.openInsertOverlay = openInsertOverlay;

    /* Functions bound to view model */

    function save() {

      vm.page.saveButtonState = "busy";
      vm.partialViewMacro.content = vm.editor.getValue();

      contentEditingHelper.contentEditorPerformSave({
        saveMethod: codefileResource.save,
        scope: $scope,
        content: vm.partialViewMacro,
        rebindCallback: function (orignal, saved) { }
      }).then(function (saved) {
        // create macro if needed
        if ($routeParams.create && $routeParams.nomacro !== "true") {
          macroResource.createPartialViewMacroWithFile(saved.virtualPath, saved.name).then(function (created) {
            navigationService.syncTree({
              tree: "macros",
              path: '-1,new',
              forceReload: true,
              activate: false
            });
            completeSave(saved);
          }, Utilities.noop);


        } else {
          completeSave(saved);
        }

      }, function (err) {

        vm.page.saveButtonState = "error";

        localizationService.localize("speechBubbles_validationFailedHeader").then(function (headerValue) {
          localizationService.localize("speechBubbles_validationFailedMessage").then(function (msgValue) {
            notificationsService.error(headerValue, msgValue);
          });
        });

      });

    }

    function completeSave(saved) {

      localizationService.localize("speechBubbles_partialViewSavedHeader").then(function (headerValue) {
        localizationService.localize("speechBubbles_partialViewSavedText").then(function (msgValue) {
          notificationsService.success(headerValue, msgValue);
        });
      });

      //check if the name changed, if so we need to redirect
      if (vm.partialViewMacro.id !== saved.id) {
        contentEditingHelper.redirectToRenamedContent(saved.id);
      }
      else {
        vm.page.saveButtonState = "success";
        vm.partialViewMacro = saved;

        //sync state
        editorState.set(vm.partialViewMacro);

        // normal tree sync
        navigationService.syncTree({ tree: "partialViewMacros", path: vm.partialViewMacro.path, forceReload: true }).then(function (syncArgs) {
          vm.page.menu.currentNode = syncArgs.node;
        });

        // clear $dirty state on form
        setFormState("pristine");
      }

    }

    function openInsertOverlay() {
      var insertOverlay = {
        allowedTypes: {
          macro: true,
          dictionary: true,
          umbracoField: true
        },
        submit: function (model) {
          switch (model.insert.type) {
            case "macro":
              var macroObject = macroService.collectValueData(model.insert.selectedMacro, model.insert.macroParams, "Mvc");
              insert(macroObject.syntax);
              break;
            case "dictionary":
              var code = templateHelper.getInsertDictionarySnippet(model.insert.node.name);
              insert(code);
              break;
            case "umbracoField":
              insert(model.insert.umbracoField);
              break;
          }
          editorService.close();
        },
        close: function (oldModel) {
          // close the dialog
          editorService.close();
          // focus editor
          vm.editor.focus();
        }
      };
      editorService.insertCodeSnippet(insertOverlay);
    }

    function openMacroOverlay() {
      var macroPicker = {
        dialogData: {},
        submit: function (model) {
          var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, "Mvc");
          insert(macroObject.syntax);
          editorService.close();
        },
        close: function () {
          editorService.close();
          vm.editor.focus();
        }
      };
      editorService.macroPicker(macroPicker);
    }


    function openPageFieldOverlay() {
      var insertFieldEditor = {
        submit: function (model) {
          insert(model.umbracoField);
          editorService.close();
        },
        close: function () {
          editorService.close();
          vm.editor.focus();
        }
      };
      editorService.insertField(insertFieldEditor);
    }


    function openDictionaryItemOverlay() {

      var labelKeys = [
        "template_insertDictionaryItem",
        "emptyStates_emptyDictionaryTree"
      ];

      localizationService.localizeMany(labelKeys).then(function (values) {
        var title = values[0];
        var emptyStateMessage = values[1];

        var dictionaryPicker = {
          section: "translation",
          treeAlias: "dictionary",
          entityType: "dictionary",
          multiPicker: false,
          title: title,
          emptyStateMessage: emptyStateMessage,
          select: function (node) {
            var code = templateHelper.getInsertDictionarySnippet(node.name);
            insert(code);
            editorService.close();
          },
          close: function (model) {
            // close dialog
            editorService.close();
            // focus editor
            vm.editor.focus();
          }
        };

        editorService.treePicker(dictionaryPicker);

      });
    }

    function openQueryBuilderOverlay() {
      var queryBuilder = {
        submit: function (model) {
          var code = templateHelper.getQuerySnippet(model.result.queryExpression);
          insert(code);
          editorService.close();
        },
        close: function (model) {
          // close dialog
          editorService.close();
          // focus editor
          vm.editor.focus();
        }
      };
      editorService.queryBuilder(queryBuilder);
    }

    /* Local functions */

    function init() {
      //we need to load this somewhere, for now its here.
      assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css", $scope);

      if ($routeParams.create) {

        var snippet = "Empty";

        if ($routeParams.snippet) {
          snippet = $routeParams.snippet;
        }

        codefileResource.getScaffold("partialViewMacros", $routeParams.id, snippet).then(function (partialViewMacro) {
          if ($routeParams.name) {
            partialViewMacro.name = $routeParams.name;
          }
          ready(partialViewMacro, false);
        });

      } else {
        codefileResource.getByPath('partialViewMacros', $routeParams.id).then(function (partialViewMacro) {
          ready(partialViewMacro, true);
        });
      }
    }

    function ready(partialViewMacro, syncTree) {

      vm.page.loading = false;
      vm.partialViewMacro = partialViewMacro;

      //sync state
      editorState.set(vm.partialViewMacro);

      if (syncTree) {
        navigationService.syncTree({ tree: "partialViewMacros", path: vm.partialViewMacro.path, forceReload: true }).then(function (syncArgs) {
          vm.page.menu.currentNode = syncArgs.node;
        });
      }

      // ace configuration
      vm.aceOption = {
        mode: "razor",
        theme: "chrome",
        showPrintMargin: false,
        advanced: {
          fontSize: '14px'
        },
        onLoad: function (_editor) {
          vm.editor = _editor;

          // Set read-only when using runtime mode Production
          _editor.setReadOnly(vm.runtimeModeProduction);

          // initial cursor placement
          // Keep cursor in name field if we are create a new template
          // else set the cursor at the bottom of the code editor
          if (!$routeParams.create) {
            $timeout(function () {
              vm.editor.navigateFileEnd();
              vm.editor.focus();
              persistCurrentLocation();
            });
          }

          //change on blur, focus
          vm.editor.on("blur", persistCurrentLocation);
          vm.editor.on("focus", persistCurrentLocation);
          vm.editor.on("change", changeAceEditor);

        }
      }

    }

    function insert(str) {
      vm.editor.focus();
      vm.editor.moveCursorToPosition(vm.currentPosition);
      vm.editor.insert(str);

      // set form state to $dirty
      setFormState("dirty");
    }

    function persistCurrentLocation() {
      vm.currentPosition = vm.editor.getCursorPosition();
    }

    function changeAceEditor() {
      setFormState("dirty");
    }

    function setFormState(state) {

      // get the current form
      var currentForm = angularHelper.getCurrentForm($scope);

      // set state
      if (state === "dirty") {
        currentForm.$setDirty();
      } else if (state === "pristine") {
        currentForm.$setPristine();
      }
    }


    init();

  }

  angular.module("umbraco").controller("Umbraco.Editors.PartialViewMacros.EditController", partialViewMacrosEditController);
})();
