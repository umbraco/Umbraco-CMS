(function () {
    "use strict";

    function MemberAppContentController($scope) {

        var vm = this;

        vm.hideSystemProperties = function (property) {
            // hide some specific, known properties by alias
            if (property.alias === "_umb_id" || property.alias === "_umb_doctype") {
                return false;
            }
            // hide all label properties with the alias prefix "umbracoMember" (e.g. "umbracoMemberFailedPasswordAttempts")
            return property.view !== "readonlyvalue" || property.alias.startsWith('umbracoMember') === false;
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.Member.Apps.ContentController", MemberAppContentController);
})();
