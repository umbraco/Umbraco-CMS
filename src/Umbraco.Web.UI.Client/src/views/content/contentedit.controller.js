/**
 * @ngdoc controller
 * @name ContentEditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, contentResource, notificationsService, angularHelper, serverValidationService) {


    /**
     * @ngdoc function
     * @name handleValidationErrors
     * @methodOf ContentEditController
     * @function
     *
     * @description
     * A function to handle the validation (ModelState) errors collection which will happen on a 403 error indicating validation errors
     */
    function handleValidationErrors(modelState) {
        //get a list of properties since they are contained in tabs
        var allProps = [];
        for (var i = 0; i < $scope.content.tabs.length; i++) {
            for (var p = 0; p < $scope.content.tabs[i].properties.length; p++) {
                allProps.push($scope.content.tabs[i].properties[p]);
            }
        }

        for (var e in modelState) {
            //the alias in model state can be in dot notation which indicates
            // * the first part is the content property alias
            // * the second part is the field to which the valiation msg is associated with
            //There will always be at least 2 parts since all model errors for properties are prefixed with "Properties"
            var parts = e.split(".");
            if (parts.length > 1) {
                var propertyAlias = parts[1];

                //find the content property for the current error
                var contentProperty = _.find(allProps, function(item) {
                    return (item.alias === propertyAlias);
                });

                if (contentProperty) {
                    //if it contains 2 '.' then we will wire it up to a property's field
                    if (parts.length > 2) {
                        //add an error with a reference to the field for which the validation belongs too
                        serverValidationService.addPropertyError(contentProperty, parts[2], modelState[e][0]);
                    }
                    else {
                        //add a generic error for the property, no reference to a specific field
                        serverValidationService.addPropertyError(contentProperty, "", modelState[e][0]);
                    }
                }
            }
            else {
                //the parts are only 1, this means its not a property but a native content property
                serverValidationService.addFieldError(parts[0], modelState[e][0]);
            }
            
            //add to notifications
            notificationsService.error("Validation", modelState[e][0]);
        }
    }

    /**
     * @ngdoc function
     * @name handleSaveError
     * @methodOf ContentEditController
     * @function
     *
     * @description
     * A function to handle what happens when we have validation issues from the server side
     */
    function handleSaveError(err) {
        //When the status is a 403 status, we have validation errors.
        //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
        //Or, some strange server error
        if (err.status == 403) {
            //now we need to look through all the validation errors
            if (err.data && (err.data.ModelState)) {
                
                handleValidationErrors(err.data.ModelState);
            }
        }
        else {
            //TODO: Implement an overlay showing the full YSOD like we had in v5
            notificationsService.error("Validation failed", err);
        }
    }

    //get the data to show, scaffold for new or get existing
    if ($routeParams.create) {
        contentResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function(data) {
                $scope.contentLoaded = true;
                $scope.content = data;
            });
    }
    else {
        contentResource.getById($routeParams.id)
            .then(function(data) {
                $scope.contentLoaded = true;
                $scope.content = data;
            });
    }

    $scope.files = [];
    $scope.addFiles = function (propertyId, files) {
        //this will clear the files for the current property and then add the new ones for the current property
        $scope.files = _.reject($scope.files, function (item) {
            return item.id == propertyId;
        });
        for (var i = 0; i < files.length; i++) {
            //save the file object to the scope's files collection
            $scope.files.push({ id: propertyId, file: files[i] });
        }
    };

    //ensure there is a form object assigned.
    var currentForm = angularHelper.getRequiredCurrentForm($scope);

    //TODO: Need to figure out a way to share the saving and event broadcasting with all editors!

    $scope.saveAndPublish = function (cnt) {

        $scope.$broadcast("saving", { scope: $scope });

        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationService.reset();
        
        contentResource.publishContent(cnt, $routeParams.create, $scope.files)
            .then(function(data) {
                $scope.content = data;
                notificationsService.success("Published", "Content has been saved and published");                
                $scope.$broadcast("saved", { scope: $scope });
            }, function(err) {
                handleSaveError(err);
            });	        
    };

    $scope.save = function (cnt) {
	        
        $scope.$broadcast("saving", { scope: $scope });
            
        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationService.reset();

        contentResource.saveContent(cnt, $routeParams.create, $scope.files)
            .then(function (data) {
                $scope.content = data;
                notificationsService.success("Saved", "Content has been saved");                
                $scope.$broadcast("saved", { scope: $scope });
            }, function(err) {
                handleSaveError(err);
            });
	        
    };
}

angular.module("umbraco").controller("Umbraco.Editors.ContentEditController", ContentEditController);
