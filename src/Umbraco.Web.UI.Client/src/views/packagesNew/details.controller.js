(function () {
    "use strict";

    function PackageDetailsController($scope, $routeParams) {

        var vm = this;

        vm.page = {};

        vm.package = {
            "name": "Merchello",
            "description": "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur dignissim purus pulvinar odio iaculis, sit amet euismod arcu volutpat. Sed ut hendrerit sem. Vestibulum enim nisl, luctus quis cursus et, porttitor a ligula. Donec sed congue urna. Integer tincidunt ultrices lorem vitae suscipit. Sed non turpis massa. Donec et velit ante. Sed interdum lectus id lorem congue, sit amet lacinia ex placerat. In id orci sed augue cursus sodales.",
            "compatibility": [
                {
                    "version": "7.4.x",
                    "compatibility": "100%"
                },
                {
                    "version": "7.3.x",
                    "compatibility": "86%"
                },
                {
                    "version": "7.2.x",
                    "compatibility": "93%"
                },
                {
                    "version": "7.1.x",
                    "compatibility": "100%"
                },
                {
                    "version": "7.0.x",
                    "compatibility": "untested"
                },
                {
                    "version": "6.1.x",
                    "compatibility": "untested"
                },
                {
                    "version": "6.0.x",
                    "compatibility": "untested"
                },
                {
                    "version": "4.11.x",
                    "compatibility": "untested"
                },
                {
                    "version": "4.10.x",
                    "compatibility": "untested"
                },
                {
                    "version": "4.9.1",
                    "compatibility": "untested"
                },
                {
                    "version": "4.9.0",
                    "compatibility": "untested"
                }
            ],
            "information": {
                "owner": "Rusty Swayne",
                "contributors": [
                    {
                        "name": "Lee"
                    },
                    {
                        "name": "Jason Prothero"
                    }
                ],
                "created": "18/12/2013",
                "currentVersion": "2.0.0",
                ".netVersion": "4.5",
                "license": "MIT",
                "downloads": "4198",
                "karma": "53"
            },
            "externalSources": [
                {
                    "name": "Source code",
                    "url": "https://github.com/merchello/Merchello"
                },
                {
                    "name": "Issue tracker",
                    "url": "http://issues.merchello.com/youtrack/oauth?state=%2Fyoutrack%2FrootGo"
                }
            ]
        };

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.DetailsController", PackageDetailsController);

})();
