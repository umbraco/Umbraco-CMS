angular.module("umbraco")

//this controller is obsolete and should not be used anymore
//it proxies everything to the system media list view which has overtaken
//all the work this property editor used to perform
.controller("Umbraco.PropertyEditors.FolderBrowserController",
    function ($rootScope, $scope, contentTypeResource) {
        //get the system media listview
        contentTypeResource.getPropertyTypeScaffold(-96)
            .then(function(dt) {

                $scope.fakeProperty = {
                    alias: "contents",
                    config: dt.config,
                    description: "",
                    editor: dt.editor,
                    hideLabel: true,
                    id: 1,
                    label: "Contents:",
                    validation: {
                        mandatory: false,
                        pattern: null
                    },
                    value: "",
                    view: dt.view
                };

        });
});
