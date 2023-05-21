/**
@ngdoc directive
@name umbraco.directives.directive:umbKeyboardList
@restrict E

@description
<b>Added in versions 7.7.0</b>: Use this directive to add arrow up and down keyboard shortcuts to a list. Use this together with the {@link umbraco.directives.directive:umbDropdown umbDropdown} component to make easy accessible dropdown menus.

<h3>Markup example</h3>
<pre>
    <div>
        <ul umb-keyboard-list>
            <li><button type="button" class="btn-reset">Item 1</button></li>
            <li><button type="button" class="btn-reset">Item 2</button></li>
            <li><button type="button" class="btn-reset">Item 3</button></li>
            <li><button type="button" class="btn-reset">Item 4</button></li>
            <li><button type="button" class="btn-reset">Item 5</button></li>
            <li><button type="button" class="btn-reset">Item 6</button></li>
        </ul>
    </div>
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbDropdown umbDropdown}</li>
</ul>

**/

angular.module('umbraco.directives')
    .directive('umbKeyboardList', ['$document', '$timeout', function ($document, $timeout) {

        return {
            restrict: 'A',
            link: function (scope, element) {

                var listItems = [];
                var currentIndex = 0;
                var focusSet = false;

                $timeout(function() {
                    // get list of all links in the list
                    listItems = element.find("li :tabbable");
                });

                // Handle keydown events
                function keydown(event) {
                    $timeout(function() {
                        checkFocus();
                        // arrow down
                        if (event.keyCode === 40) {
                            arrowDown();
                        }
                        // arrow up
                        if (event.keyCode === 38) {
                            arrowUp();
                        }
                    });
                }

                function checkFocus() {
                    var found = false;

                    // check if any element has focus
                    Utilities.forEach(listItems, (item, index) => {
                        if ($(item).is(":focus")) {
                            // if an element already has focus set the
                            // currentIndex so we navigate from that element
                            currentIndex = index;
                            focusSet = true;
                            found = true;
                        }
                    });

                    // If we don't find an element with focus we reset the currentIndex and the focusSet flag
                    // we do this because you can have navigated away from the list with tab and we want to reset it if you navigate back
                    if (!found) {
                        currentIndex = 0;
                        focusSet = false;
                    }
                }

                function arrowDown() {
                    if (currentIndex < listItems.length - 1) {
                        // only bump the current index if the focus is already
                        // set else we just want to focus the first element
                        if (focusSet) {
                            currentIndex++;
                        }
                        listItems[currentIndex].trigger("focus");
                        focusSet = true;
                    }
                }

                function arrowUp() {
                    if (currentIndex > 0) {
                        currentIndex--;
                        listItems[currentIndex].trigger("focus");
                    }
                }

                // Stop to listen typing.
                function stopListening() {
                    $document.off('keydown', keydown);
                }

                // Start listening to key typing.
                $document.on('keydown', keydown);

                // Stop listening when scope is destroyed.
                scope.$on('$destroy', stopListening);
            }
        };
    }]);
