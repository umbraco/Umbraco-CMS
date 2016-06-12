(function () {
    "use strict";

    function TemplatesEditController($scope) {

        var vm = this;

        vm.page = {};
        vm.page.loading = false;

        vm.mode = "css";

        vm.aceOption = {
            mode: vm.mode.toLowerCase(),
            onLoad: function(ace) {

                console.log(ace);

                // HACK to have the ace instance in the scope...
                /*
                $scope.modeChanged = function() {
                    _ace.getSession().setMode("ace/mode/" + $scope.mode.toLowerCase());
                };
                */

            }
        };

        vm.aceModel = ';; Scheme code in here.\n' +
            '(define (double x)\n\t(* x x))\n\n\n' +
            '<!-- XML code in here. -->\n' +
            '<root>\n\t<foo>\n\t</foo>\n\t<bar/>\n</root>\n\n\n' +
            '// Javascript code in here.\n' +
            'function foo(msg) {\n\tvar r = Math.random();\n\treturn "" + r + " : " + msg;\n}';

        vm.openPageFieldOverlay = openPageFieldOverlay;
        vm.openDictionaryItemOverlay = openDictionaryItemOverlay;
        vm.openQueryBuilderOverlay = openQueryBuilderOverlay;

        function init() {


        }

        function openPageFieldOverlay() {
            vm.pageFieldOverlay = {
                view: "mediapicker",
                show: true,
                submit: function(model) {

                },
                close: function(model) {
                    vm.pageFieldOverlay.show = false;
                    vm.pageFieldOverlay = null;
                }
            };
        }

        function openDictionaryItemOverlay() {
            vm.dictionaryItemOverlay = {
                view: "mediapicker",
                show: true,
                submit: function(model) {

                },
                close: function(model) {
                    vm.dictionaryItemOverlay.show = false;
                    vm.dictionaryItemOverlay = null;
                }
            };
        }

        function openQueryBuilderOverlay() {
            vm.queryBuilderOverlay = {
                view: "mediapicker",
                show: true,
                submit: function(model) {

                },
                close: function(model) {
                    vm.queryBuilderOverlay.show = false;
                    vm.queryBuilderOverlay = null;
                }
            };
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Templates.EditController", TemplatesEditController);
})();
