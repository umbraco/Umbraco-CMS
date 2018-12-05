(function () {
    "use strict";

    function EditController($location) {

        const vm = this;

        vm.showBackButton = true;

        // open all expansion panels
        vm.propertiesOpen = true;
        vm.contentOpen = true;
        vm.filesOpen = true;
        vm.actionsOpen = true;

        vm.back = back;
        vm.createPackage = createPackage;
        vm.save = save;

        function onInit() {
            // load package

            vm.package = {
                "name": "My package"
            };

            vm.documentTypes = [
                {
                    "name": "Home"
                },
                {
                    "name": "Blog"
                },
                {
                    "name": "Blog Post"
                },
                {
                    "name": "News"
                },
                {
                    "name": "News Item"
                },
                {
                    "name": "Contact"
                },
                {
                    "name": "About"
                }
            ];

            vm.templates = [
                {
                    "name": "Home"
                },
                {
                    "name": "Blog"
                },
                {
                    "name": "Blog Post"
                },
                {
                    "name": "News"
                },
                {
                    "name": "News Item"
                },
                {
                    "name": "Contact"
                },
                {
                    "name": "About"
                }
            ];

            vm.stylesheets = [
                {
                    "name": "styles.css"
                },
                {
                    "name": "carousel.css"
                },
                {
                    "name": "fonts.css"
                }
            ];

            vm.macros = [
                {
                    "name": "Macro 1"
                },
                {
                    "name": "Macro 2"
                },
                {
                    "name": "Macro 3"
                }
            ];

            vm.languages = [
                {
                    "name": "English (United States)"
                },
                {
                    "name": "Danish"
                },
                {
                    "name": "Spanish"
                }
            ];

            vm.dictionaryItems = [
                {
                    "name": "Item 1"
                },
                {
                    "name": "Item 2"
                },
                {
                    "name": "Item 3"
                }
            ];

            vm.dataTypes = [
                {
                    "name": "Datatype 1"
                },
                {
                    "name": "Datatype 2"
                },
                {
                    "name": "Datatype 3"
                }
            ];
        }

        function back() {
            $location.path("packages/packages/overview");
        }

        function createPackage() {
            console.log("create package");
        }

        function save() {
            console.log("save package");
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.EditController", EditController);

})();
