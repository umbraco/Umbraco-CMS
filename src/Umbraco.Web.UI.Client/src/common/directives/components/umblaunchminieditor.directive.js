/**
* @ngdoc directive
* @name umbraco.directives.directive:umbLaunchMiniEditor 
* @restrict E
* @function
* @description 
* Used on a button to launch a mini content editor editor dialog
**/
angular.module("umbraco.directives")
    .directive('umbLaunchMiniEditor', function (dialogService, editorState, fileManager, contentEditingHelper) {
        return {
            restrict: 'A',
            replace: false,
            scope: {
                node: '=umbLaunchMiniEditor',
            },
            link: function(scope, element, attrs) {

                var launched = false;

                element.click(function() {

                    if (launched === true) {
                        return;
                    }

                    launched = true;

                    //We need to store the current files selected in the file manager locally because the fileManager
                    // is a singleton and is shared globally. The mini dialog will also be referencing the fileManager 
                    // and we don't want it to be sharing the same files as the main editor. So we'll store the current files locally here,
                    // clear them out and then launch the dialog. When the dialog closes, we'll reset the fileManager to it's previous state.
                    var currFiles = _.groupBy(fileManager.getFiles(), "alias");
                    fileManager.clearFiles();

                    //We need to store the original editorState entity because it will need to change when the mini editor is loaded so that
                    // any property editors that are working with editorState get given the correct entity, otherwise strange things will 
                    // start happening.
                    var currEditorState = editorState.getCurrent();

                    dialogService.open({
                        template: "views/common/dialogs/content/edit.html",
                        id: scope.node.id,
                        closeOnSave: true,
                        tabFilter: ["Generic properties"],
                        callback: function (data) {

                            //set the node name back
                            scope.node.name = data.name;

                            //reset the fileManager to what it was
                            fileManager.clearFiles();
                            _.each(currFiles, function (val, key) {
                                fileManager.setFiles(key, _.map(currFiles['upload'], function (i) { return i.file; }));
                            });

                            //reset the editor state
                            editorState.set(currEditorState);

                            //Now we need to check if the content item that was edited was actually the same content item
                            // as the main content editor and if so, update all property data	                
                            if (data.id === currEditorState.id) {
                                var changed = contentEditingHelper.reBindChangedProperties(currEditorState, data);
                            }

                            launched = false;
                        },
                        closeCallback: function () {
                            //reset the fileManager to what it was
                            fileManager.clearFiles();
                            _.each(currFiles, function (val, key) {
                                fileManager.setFiles(key, _.map(currFiles['upload'], function (i) { return i.file; }));
                            });

                            //reset the editor state
                            editorState.set(currEditorState);

                            launched = false;
                        }
                    });

                });

            }
        };
    });