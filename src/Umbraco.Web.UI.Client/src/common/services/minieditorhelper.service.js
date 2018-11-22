(function () {
    'use strict';

    function miniEditorHelper(dialogService, editorState, fileManager, contentEditingHelper, $q) {

        var launched = false;

        function launchMiniEditor(node) {

            var deferred = $q.defer();

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
                id: node.id,
                closeOnSave: true,
                tabFilter: ["Generic properties"],
                callback: function (data) {

                    //set the node name back
                    node.name = data.name;

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
                    
                    deferred.resolve(data);

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

                    deferred.reject();

                }
            });

            return deferred.promise;

        }

        var service = {
            launchMiniEditor: launchMiniEditor
        };

        return service;

    }


    angular.module('umbraco.services').factory('miniEditorHelper', miniEditorHelper);


})();
