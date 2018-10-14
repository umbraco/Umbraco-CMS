(function () {
    "use strict";

    function ContentAssignDomainController(
        $scope,
        contentResource,
        notificationsService,
        navigationService,
        localizationService,
        formHelper,
        contentEditingHelper) {

        $scope.sortableOptions = {
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            items: '> tr',
            tolerance: 'pointer',
            update: function (e, ui) {
                // Get the new and old index for the moved element (using the text as the identifier, so 
                // we'd have a problem if two prevalues were the same, but that would be unlikely)
                var newIndex = ui.item.index();
                var movedDomainName = $('input[type="text"]', ui.item).val();
                var originalIndex = getElementIndexByDomainName(movedDomainName);

                // Move the element in the model
                if (originalIndex > -1) {
                    var movedElement = $scope.model.Domains[originalIndex];
                    $scope.model.Domains.splice(originalIndex, 1);
                    $scope.model.Domains.splice(newIndex, 0, movedElement);
                }
            }
        };

        function getElementIndexByDomainName(value) {
            for (var i = 0; i < $scope.model.Domains.length; i++) {
                if ($scope.model.Domains[i].Name === value) {
                    return i;
                }
            }

            return -1;
        }

        $scope.remove = function (item, evt) {
            evt.preventDefault();

            $scope.model.Domains = _.reject($scope.model.Domains, function (x) {
                return x.Name === item.Name;
            });
        };

        $scope.add = function (evt) {
            evt.preventDefault();

            $scope.model.Domains.push({
                Name: "",
                Lang: $scope.model.AvailableLanguages[0],
                Sort: $scope.model.Domains.length
            })
        };

        $scope.cancel = function () {
            navigationService.hideMenu();
        };

        $scope.save = function () {

            // Update sort value
            for (var i = 0; i < $scope.model.Domains.length; i++) {
                $scope.model.Domains[i].SortOrder = i;
            }

            if (formHelper.submitForm({
                scope: $scope,
                formCtrl: this.languageAndDomainsForm,
                statusMessage: "Saving language and domains..."
            })) {

                contentResource.saveLanguageAndDomains($scope.model)
                    .then(function (data) {

                        formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                        navigationService.hideMenu();
                    },
                    function (err) {

                        contentEditingHelper.handleSaveError({
                            redirectOnFailure: false,
                            err: err
                        });

                    }
                );
            }
        };

        function init() {
            $scope.loading = true;

            contentResource.getLanguageAndDomains($scope.$parent.currentNode.id).then(function (languageAndDomains) {
                $scope.model = languageAndDomains;
                $scope.loading = false;
            });
        };


        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.AssignDomainController", ContentAssignDomainController);

})();
