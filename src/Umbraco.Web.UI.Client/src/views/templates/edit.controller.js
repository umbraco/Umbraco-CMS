(function () {
  "use strict";

  function TemplatesEditController($scope, $routeParams, $timeout, templateResource, assetsService, notificationsService, editorState, navigationService, appState, macroService, treeService, contentEditingHelper, localizationService, angularHelper, templateHelper, editorService) {

    var vm = this;
    var oldMasterTemplateAlias = null;
    var infiniteMode = $scope.model && $scope.model.infiniteMode;
    var id = infiniteMode ? $scope.model.id : $routeParams.id;
    var create = infiniteMode ? $scope.model.create : $routeParams.create;

    vm.runtimeModeProduction = Umbraco.Sys.ServerVariables.application.runtimeMode == 'Production';

    vm.header = {};
    vm.header.editorfor = "template_template";
    vm.header.setPageTitle = true;

    vm.page = {};
    vm.page.loading = true;
    vm.templates = [];

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
        labelKey: "template_insertPartialView",
        addEllipsis: "true",
        handler: function () {
          vm.openPartialOverlay();
        }
      },
      {
        labelKey: "template_insertDictionaryItem",
        addEllipsis: "true",
        handler: function () {
          vm.openDictionaryItemOverlay();
        }
      },
      {
        labelKey: "template_insertMacro",
        addEllipsis: "true",
        handler: function () {
          vm.openMacroOverlay()
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
    templateHelper.getTemplateEditorShortcuts().then(function (data) {
      vm.page.keyboardShortcutsOverview.push(data);
    });

    vm.save = function (suppressNotification) {
      vm.page.saveButtonState = "busy";

      vm.template.content = vm.editor.getValue();

      contentEditingHelper.contentEditorPerformSave({
        saveMethod: templateResource.save,
        scope: $scope,
        content: vm.template,
        rebindCallback: function (orignal, saved) { }
      }).then(function (saved) {

        if (!suppressNotification) {
          localizationService.localizeMany(["speechBubbles_templateSavedHeader", "speechBubbles_templateSavedText"]).then(function (data) {
            var header = data[0];
            var message = data[1];
            notificationsService.success(header, message);
          });
        }

        vm.page.saveButtonState = "success";
        vm.template = saved;

        //sync state
        if (!infiniteMode) {
          editorState.set(vm.template);
        }

        // sync tree
        // if master template alias has changed move the node to it's new location
        if (!infiniteMode && oldMasterTemplateAlias !== vm.template.masterTemplateAlias) {

          // When creating a new template the id is -1. Make sure We don't remove the root node.
          if (vm.page.menu.currentNode.id !== "-1") {
            // move node to new location in tree
            //first we need to remove the node that we're working on
            treeService.removeNode(vm.page.menu.currentNode);
          }

          // update stored alias to the new one so the node won't move again unless the alias is changed again
          oldMasterTemplateAlias = vm.template.masterTemplateAlias;

          navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true, activate: true }).then(function (args) {
            vm.page.menu.currentNode = args.node;
          });

        } else {

          // normal tree sync
          if (!infiniteMode) {
            navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
              vm.page.menu.currentNode = syncArgs.node;
            });
          }

        }

        // clear $dirty state on form
        setFormState("pristine");

        if (infiniteMode) {
          submit();
        }


      }, function (err) {
        if (suppressNotification) {
          vm.page.saveButtonState = "error";

          localizationService.localizeMany(["speechBubbles_validationFailedHeader", "speechBubbles_validationFailedMessage"]).then(function (data) {
            var header = data[0];
            var message = data[1];
            notificationsService.error(header, message);
          });
        }
      });

    };

    vm.init = function () {

      // we need to load this somewhere, for now its here.
      assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css", $scope);

      // load templates - used in the master template picker
      templateResource.getAll()
        .then(function (templates) {
          vm.templates = templates;
        });

      if (create) {
        templateResource.getScaffold((id)).then(function (template) {
          vm.ready(template);
        });
      } else {
        templateResource.getById(id).then(function (template) {
          vm.ready(template);
        });
      }

    };


    vm.ready = function (template) {
      vm.page.loading = false;
      vm.template = template;

      // if this is a new template, bind to the blur event on the name
      if (create) {
        $timeout(function () {
          var nameField = $('[data-element="editor-name-field"]');
          if (nameField) {
            nameField.on('blur', function (event) {
              if (event.target.value) {
                vm.save(true);
              }
            });
          }
        });
      }

      // sync state
      if (!infiniteMode) {
        editorState.set(vm.template);
        navigationService.syncTree({ tree: "templates", path: vm.template.path, forceReload: true }).then(function (syncArgs) {
          vm.page.menu.currentNode = syncArgs.node;
        });
      }

      // save state of master template to use for comparison when syncing the tree on save
      oldMasterTemplateAlias = Utilities.copy(template.masterTemplateAlias);

      // ace configuration
      vm.aceOption = {
        mode: "razor",
        theme: "chrome",
        showPrintMargin: false,
        advanced: {
          fontSize: '14px',
          enableSnippets: false, //The Razor mode snippets are awful (Need a way to override these)
          enableBasicAutocompletion: true,
          enableLiveAutocompletion: false
        },
        onLoad: function (_editor) {
          vm.editor = _editor;

          // Set read-only when using runtime mode Production
          _editor.setReadOnly(vm.runtimeModeProduction);

          // Update the auto-complete method to use ctrl+alt+space
          _editor.commands.bindKey("ctrl-alt-space", "startAutocomplete");

          // Unassigns the keybinding (That was previously auto-complete)
          // As conflicts with our own tree search shortcut
          _editor.commands.bindKey("ctrl-space", null);

          // Assign new keybinding
          _editor.commands.addCommands([
            // Disable (alt+shift+K)
            // Conflicts with our own show shortcuts dialog - this overrides it
            {
              name: 'unSelectOrFindPrevious',
              bindKey: 'Alt-Shift-K',
              exec: function () {
                // Toggle the show keyboard shortcuts overlay
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
              name: 'insertPartialView',
              bindKey: 'Alt-Shift-P',
              exec: function () {
                $scope.$apply(function () {
                  openPartialOverlay();
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
            },
            {
              name: 'insertSection',
              bindKey: 'Alt-Shift-S',
              exec: function () {
                $scope.$apply(function () {
                  openSectionsOverlay();
                });
              },
              readOnly: true
            },
            {
              name: 'chooseMasterTemplate',
              bindKey: 'Alt-Shift-T',
              exec: function () {
                $scope.$apply(function () {
                  openMasterTemplateOverlay();
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

          // change on blur, focus
          vm.editor.on("blur", persistCurrentLocation);
          vm.editor.on("focus", persistCurrentLocation);
          vm.editor.on("change", changeAceEditor);
        }
      }

    };

    vm.openPageFieldOverlay = openPageFieldOverlay;
    vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
    vm.openQueryBuilderOverlay = openQueryBuilderOverlay;
    vm.openMacroOverlay = openMacroOverlay;
    vm.openInsertOverlay = openInsertOverlay;
    vm.openSectionsOverlay = openSectionsOverlay;
    vm.openPartialOverlay = openPartialOverlay;
    vm.openMasterTemplateOverlay = openMasterTemplateOverlay;
    vm.selectMasterTemplate = selectMasterTemplate;
    vm.getMasterTemplateName = getMasterTemplateName;
    vm.removeMasterTemplate = removeMasterTemplate;
    vm.closeShortcuts = closeShortcuts;
    vm.submit = submit;
    vm.close = close;

    function openInsertOverlay() {
      var insertOverlay = {
        allowedTypes: {
          macro: true,
          dictionary: true,
          partial: true,
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
            case "partial":
              var code = templateHelper.getInsertPartialSnippet(model.insert.node.parentId, model.insert.node.name);
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

    function openPartialOverlay() {

      localizationService.localize("template_insertPartialView").then(function (value) {
        var title = value;

        var partialItem = {
          section: "settings",
          treeAlias: "partialViews",
          entityType: "partialView",
          multiPicker: false,
          title: title,
          filter: function (i) {
            if (i.name.indexOf(".cshtml") === -1 && i.name.indexOf(".vbhtml") === -1) {
              return true;
            }
          },
          filterCssClass: "not-allowed",
          select: function (node) {
            var code = templateHelper.getInsertPartialSnippet(node.parentId, node.name);
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

        editorService.treePicker(partialItem);
      });
    }

    function openQueryBuilderOverlay() {
      var queryBuilder = {
        submit: function (model) {
          var code = templateHelper.getQuerySnippet(model.result.queryExpression);
          insert(code);
          editorService.close();
        },
        close: function () {
          editorService.close();
          // focus editor
          vm.editor.focus();
        }
      };
      editorService.queryBuilder(queryBuilder);
    }


    function openSectionsOverlay() {
      var templateSections = {
        isMaster: vm.template.isMasterTemplate,
        submit: function (model) {

          if (model.insertType === 'renderBody') {
            var code = templateHelper.getRenderBodySnippet();
            insert(code);
          }

          if (model.insertType === 'renderSection') {
            var code = templateHelper.getRenderSectionSnippet(model.renderSectionName, model.mandatoryRenderSection);
            insert(code);
          }

          if (model.insertType === 'addSection') {
            var code = templateHelper.getAddSectionSnippet(model.sectionName);
            wrap(code);
          }

          editorService.close();

        },
        close: function (model) {
          editorService.close();
          vm.editor.focus();
        }
      }
      editorService.templateSections(templateSections);
    }

    function openMasterTemplateOverlay() {

      // make collection of available master templates
      var availableMasterTemplates = [];

      // filter out the current template and the selected master template
      vm.templates.forEach(function (template) {
        if (template.alias !== vm.template.alias && template.alias !== vm.template.masterTemplateAlias) {
          var templatePathArray = template.path.split(',');
          // filter descendant templates of current template
          if (templatePathArray.indexOf(String(vm.template.id)) === -1) {
            availableMasterTemplates.push(template);
          }
        }
      });

      const editor = {
        filterCssClass: 'not-allowed',
        filter: item => !availableMasterTemplates.some(template => template.id == item.id),
        submit: model => {
          const template = model.selection[0];
          if (template && template.alias) {
            vm.template.masterTemplateAlias = template.alias;
            setLayout(template.alias + ".cshtml");
          } else {
            vm.template.masterTemplateAlias = null;
            setLayout(null);
          }
          editorService.close();
        },
        close: () => editorService.close()
      }

      localizationService.localize("template_mastertemplate").then(title => {
        editor.title = title;

        const currentTemplate = vm.templates.find(template => template.alias == vm.template.masterTemplateAlias);
        if (currentTemplate) {
          editor.currentNode = {
            path: currentTemplate.path
          };
        }

        editorService.templatePicker(editor);
      });

    }

    function selectMasterTemplate(template) {

      if (template && template.alias) {
        vm.template.masterTemplateAlias = template.alias;
        setLayout(template.alias + ".cshtml");
      } else {
        vm.template.masterTemplateAlias = null;
        setLayout(null);
      }

    }

    function getMasterTemplateName(masterTemplateAlias, templates) {
      if (masterTemplateAlias) {
        var templateName = "";
        templates.forEach(function (template) {
          if (template.alias === masterTemplateAlias) {
            templateName = template.name;
          }
        });
        return templateName;
      }
    }

    function removeMasterTemplate() {

      vm.template.masterTemplateAlias = null;

      // call set layout with no paramters to set layout to null
      setLayout();

    }

    function setLayout(templatePath) {

      var templateCode = vm.editor.getValue();
      var newValue = templatePath;
      var layoutDefRegex = new RegExp("(@{[\\s\\S]*?Layout\\s*?=\\s*?)(\"[^\"]*?\"|null)(;[\\s\\S]*?})", "gi");

      if (newValue !== undefined && newValue !== "") {
        if (layoutDefRegex.test(templateCode)) {
          // Declaration exists, so just update it
          templateCode = templateCode.replace(layoutDefRegex, "$1\"" + newValue + "\"$3");
        } else {
          // Declaration doesn't exist, so prepend to start of doc
          // TODO: Maybe insert at the cursor position, rather than just at the top of the doc?
          templateCode = "@{\n\tLayout = \"" + newValue + "\";\n}\n" + templateCode;
        }
      } else {
        if (layoutDefRegex.test(templateCode)) {
          // Declaration exists, so just update it
          templateCode = templateCode.replace(layoutDefRegex, "$1null$3");
        }
      }

      vm.editor.setValue(templateCode);
      vm.editor.clearSelection();
      vm.editor.navigateFileStart();

      vm.editor.focus();
      // set form state to $dirty
      setFormState("dirty");

    }


    function insert(str) {
      vm.editor.focus();
      vm.editor.moveCursorToPosition(vm.currentPosition);
      vm.editor.insert(str);

      // set form state to $dirty
      setFormState("dirty");
    }

    function wrap(str) {

      var selectedContent = vm.editor.session.getTextRange(vm.editor.getSelectionRange());
      str = str.replace("{0}", selectedContent);
      vm.editor.insert(str);
      vm.editor.focus();

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

    function closeShortcuts() {
      vm.showKeyboardShortcut = false;
    }

    function submit() {
      if ($scope.model.submit) {
        $scope.model.template = vm.template;
        $scope.model.submit($scope.model);
      }
    }

    function close() {
      if ($scope.model.close) {
        $scope.model.close();
      }
    }

    vm.init();

  }

  angular.module("umbraco").controller("Umbraco.Editors.Templates.EditController", TemplatesEditController);
})();
