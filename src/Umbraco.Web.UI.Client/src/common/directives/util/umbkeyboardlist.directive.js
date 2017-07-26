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
            <li><a href="">Item 1</a></li>
            <li><a href="">Item 2</a></li>
            <li><a href="">Item 3</a></li>
            <li><a href="">Item 4</a></li>
            <li><a href="">Item 5</a></li>
            <li><a href="">Item 6</a></li>
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
            link: function (scope, element, attr) {
                
                var listItems = [];
                var currentIndex = 0;
                var focusSet = false;
            
                $timeout(function(){
                    // get list of all links in the list
                    listItems = element.find("li a");
                });

                // Handle keydown events
                function keydown(event) {

                    // arrow down
                    if (event.keyCode === 40) {
                        if(currentIndex < listItems.length - 1) {
                            // we do this to focus the first element when 
                            // using the arrow down key the first time
                            if(focusSet) {
                                currentIndex++;
                            }
                            listItems[currentIndex].focus();
                            focusSet = true;
                        }
                    }

                    // arrow up
                    if (event.keyCode === 38) {
                        if(currentIndex > 0) {
                            currentIndex--;
                            listItems[currentIndex].focus();
                        }
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