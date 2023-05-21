/**
* @ngdoc directive
* @name umbraco.directives.directive:hotkey
**/

angular.module("umbraco.directives")
    .directive('hotkey', function($window, keyboardService, $log, focusService) {

        return function(scope, el, attrs) {

            var options = {};
            var keyCombo = attrs.hotkey;

            if (!keyCombo) {
                //support data binding
                keyCombo = scope.$eval(attrs["hotkey"]);
            }

            function activate() {

                if (keyCombo) {

                    // disable shortcuts in input fields if keycombo is 1 character
                    if (keyCombo.length === 1) {
                        options = {
                            inputDisabled: true
                        };
                    }

                    keyboardService.bind(keyCombo, function() {
                        
                        focusService.rememberFocus();
                        
                        var element = $(el);
                        var activeElementType = document.activeElement.tagName;
                        var clickableElements = ["A", "BUTTON"];

                        if (element.is("a,div,button,input[type='button'],input[type='submit'],input[type='checkbox']") && !element.is(':disabled')) {

                            if (element.is(':visible') || attrs.hotkeyWhenHidden) {

                                if (attrs.hotkeyWhen && attrs.hotkeyWhen === "false") {
                                    return;
                                }

                                // when keycombo is enter and a link or button has focus - click the link or button instead of using the hotkey
                                if (keyCombo === "enter" && clickableElements.indexOf(activeElementType) === 0) {
                                    document.activeElement.trigger( "click" );
                                } else {
                                    element.trigger("click");
                                }

                            }

                        } else {
                            element.trigger("focus");
                        }

                    }, options);

                    el.on('$destroy', function() {
                        keyboardService.unbind(keyCombo);
                    });

                }

            }

            activate();

        };
    });
