    // A slightly modified version of https://github.com/myplanet/angular-deep-blur/blob/master/angular-deep-blur.js - Kudos to Ufuk Kayserilioglu (paracycle)
    (function () {
        'use strict';

        function DeepBlurDirective($timeout){

            function controller($scope, $element, $attrs) {
                var leaveExpr = $attrs.deepBlur,
                    dom = $element[0];

                function containsDom(parent, dom) {
                    while (dom) {
                        if (dom === parent) {
                            return true;
                        }
                        dom = dom.parentNode;
                    }
                    return false;
                }

                function onBlur(e) {
                    var targetElement = e.relatedTarget;

                    if (!containsDom(dom, targetElement)) {
                        $timeout(function () {
                            $scope.$apply(leaveExpr);
                        }, 10);
                    }
                }

                dom.addEventListener('blur', onBlur, true);
            }

            var directive = {
                restrict: 'A',
                controller: controller
            };

            return directive;

        }

        angular.module('umbraco.directives').directive('deepBlur', DeepBlurDirective);

    })();
