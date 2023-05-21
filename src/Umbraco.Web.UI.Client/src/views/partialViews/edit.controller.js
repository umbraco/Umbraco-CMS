(function () {
  "use strict";

  function PartialViewsEditController($scope, $routeParams, codefileResource, assetsService, notificationsService, editorState, navigationService, appState, macroService, angularHelper, $timeout, contentEditingHelper, localizationService, templateHelper, editorService) {

    var vm = this;
    var infiniteMode = $scope.model && $scope.model.infiniteMode;
    var id = infiniteMode ? $scope.model.id : $routeParams.id;
    var create = infiniteMode ? $scope.model.create : $routeParams.create;
    var snippet = infiniteMode ? $scope.model.snippet : $routeParams.snippet;

    function close() {
      if ($scope.model.close) {
        $scope.model.close($scope.model);
      }
    }

    vm.close = close;

    vm.runtimeModeProduction = Umbraco.Sys.ServerVariables.application.runtimeMode == 'Production';

    vm.header = {};
    vm.header.editorfor = "visuallyHiddenTexts_newPartialView";
    vm.header.setPageTitle = true;

    vm.page = {};
    vm.page.loading = true;
    vm.partialView = {};

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

    //Used to toggle the keyboard shortcut modal
    //From a custom keybinding in ace editor - that conflicts with our own to show the dialog
    vm.showKeyboardShortcut = false;

    //Keyboard shortcuts for help dialog
    vm.page.keyboardShortcutsOverview = [];

    templateHelper.getGeneralShortcuts().then(function (data) {
      vm.page.keyboardShortcutsOverview.push(data);
    });
    templateHelper.getEditorShortcuts().then(function (data) {
      vm.page.keyboardShortcutsOverview.push(data);
    });
    templateHelper.getPartialViewEditorShortcuts().then(function (data) {
      vm.page.keyboardShortcutsOverview.push(data);
    });


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
      vm.partialView.content = vm.editor.getValue();

      contentEditingHelper.contentEditorPerformSave({
        saveMethod: codefileResource.save,
        scope: $scope,
        content: vm.partialView,
        rebindCallback: function (orignal, saved) { }
      }).then(function (saved) {

        localizationService.localize("speechBubbles_partialViewSavedHeader").then(function (headerValue) {
          localizationService.localize("speechBubbles_partialViewSavedText").then(function (msgValue) {
            notificationsService.success(headerValue, msgValue);
          });
        });

        //check if the name changed, if so we need to redirect
        if (vm.partialView.id !== saved.id) {
          contentEditingHelper.redirectToRenamedContent(saved.id);
        }
        else {
          vm.page.saveButtonState = "success";
          vm.partialView = saved;

          //sync state
          editorState.set(vm.partialView);

          // normal tree sync
          navigationService.syncTree({ tree: "partialViews", path: vm.partialView.path, forceReload: true }).then(function (syncArgs) {
            vm.page.menu.currentNode = syncArgs.node;
          });

          // clear $dirty state on form
          setFormState("pristine");
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
        close: function () {
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

        var dictionaryItem = {
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
        editorService.treePicker(dictionaryItem);
      });
    }

    function openQueryBuilderOverlay() {
      var queryBuilder = {
        title: "Query for content",
        submit: function (model) {
          var code = templateHelper.getQuerySnippet(model.result.queryExpression);
          insert(code);
          editorService.close();
        },
        close: function () {
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

      if (create) {

        if (!snippet) {
          snippet = "Empty";
        }

        codefileResource.getScaffold("partialViews", id, snippet).then(function (partialView) {
          ready(partialView, false);
        });

      } else {
        codefileResource.getByPath('partialViews', id).then(function (partialView) {
          ready(partialView, true);
        });
      }

    }

    function ready(partialView, syncTree) {

      vm.page.loading = false;
      vm.partialView = partialView;

      //sync state
      editorState.set(vm.partialView);

      if (!infiniteMode && syncTree) {
        navigationService.syncTree({ tree: "partialViews", path: vm.partialView.path, forceReload: true }).then(function (syncArgs) {
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

          //Update the auto-complete method to use ctrl+alt+space
          _editor.commands.bindKey("ctrl-alt-space", "startAutocomplete");

          //Unassigns the keybinding (That was previously auto-complete)
          //As conflicts with our own tree search shortcut
          _editor.commands.bindKey("ctrl-space", null);

          // Assign new keybinding
          _editor.commands.addCommands([
            //Disable (alt+shift+K)
            //Conflicts with our own show shortcuts dialog - this overrides it
            {
              name: 'unSelectOrFindPrevious',
              bindKey: 'Alt-Shift-K',
              exec: function () {
                //Toggle the show keyboard shortcuts overlay
                $scope.$apply(function () {
                  vm.showKeyboardShortcut = !vm.showKeyboardShortcut;
                });
              },
              readOnly: true
            },
            {
              name: 'insertUmbracoValue',
              bindKey: 'Alt-Shift-V',
              exec: function () {
                $scope.$apply(function () {
                  openPageFieldOverlay();
                });
              },
              readOnly: true
            },
            {
              name: 'insertDictionary',
              bindKey: 'Alt-Shift-D',
              exec: function () {
                $scope.$apply(function () {
                  openDictionaryItemOverlay();
                });
              },
              readOnly: true
            },
            {
              name: 'insertUmbracoMacro',
              bindKey: 'Alt-Shift-M',
              exec: function () {
                $scope.$apply(function () {
                  openMacroOverlay();
                });
              },
              readOnly: true
            },
            {
              name: 'insertQuery',
              bindKey: 'Alt-Shift-Q',
              exec: function () {
                $scope.$apply(function () {
                  openQueryBuilderOverlay();
                });
              },
              readOnly: true
            }

          ]);

          // initial cursor placement
          // Keep cursor in name field if we are create a new template
          // else set the cursor at the bottom of the code editor
          if (!create) {
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

  angular.module("umbraco").controller("Umbraco.Editors.PartialViews.EditController", PartialViewsEditController);
})();
