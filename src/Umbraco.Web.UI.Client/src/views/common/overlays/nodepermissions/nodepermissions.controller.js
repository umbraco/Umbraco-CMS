(function () {
    "use strict";

    function NodePermissionsController($scope) {

        var vm = this;

        function onInit() {

            // fake node permissions
            $scope.model.node.permissions = [
                {
                    "groupName": "Group 1",
                    "permissions": [
                        {
                            "name": "Edit content (save)",
                            "description": "Lorem ipsum dolor sit amet",
                            "checked": false
                        },
                        {
                            "name": "Browse content",
                            "description": "Nullam egestas porta mi, quis finibus nisl commodo a",
                            "checked": true
                        },
                        {
                            "name": "Publish",
                            "description": "Aliquam molestie consequat felis",
                            "checked": true
                        },
                        {
                            "name": "Send to publish",
                            "description": "Sed pharetra sodales enim quis molestie",
                            "checked": true
                        },
                        {
                            "name": "Delete",
                            "description": "Vitae porta mauris turpis sit amet ligula",
                            "checked": true
                        },
                        {
                            "name": "Create",
                            "description": "Vestibulum pretium sapien id turpis elementum viverra",
                            "checked": true
                        },
                    ]
                },
                {
                    "groupName": "Group 2",
                    "permissions": [
                        {
                            "name": "Move",
                            "description": "Vestibulum pretium sapien id turpis elementum viverra",
                            "checked": true
                        },
                        {
                            "name": "Copy",
                            "description": "Phasellus sagittis, dolor vel accumsan porttitor",
                            "checked": false
                        },
                        {
                            "name": "Sort",
                            "description": "Aliquam erat volutpat",
                            "checked": false
                        }
                    ]
                },
                {
                    "groupName": "Group 3",
                    "permissions": [
                        {
                            "name": "Culture and Hostnames",
                            "description": "Lorem ipsum dolor sit amet",
                            "checked": true
                        },
                        {
                            "name": "Audit Trail",
                            "description": "Lorem ipsum dolor sit amet",
                            "checked": true
                        },
                        {
                            "name": "Translate",
                            "description": "Lorem ipsum dolor sit amet",
                            "checked": true
                        },
                        {
                            "name": "Change document type",
                            "description": "Lorem ipsum dolor sit amet",
                            "checked": true
                        },
                        {
                            "name": "Public access",
                            "description": "Lorem ipsum dolor sit amet",
                            "checked": true
                        },
                        {
                            "name": "Rollback",
                            "description": "Lorem ipsum dolor sit amet",
                            "checked": true
                        }
                    ]
                }
            ];
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.NodePermissionsController", NodePermissionsController);

})();
