(function () {
    'use strict';

    /** This directive is used to render out the current variant tabs and properties and exposes an API for other directives to consume  */
    function tabbedContentDirective($timeout) {

        function link($scope, $element) {
            
            var appRootNode = $element[0];
            
            // Directive for cached property groups.
            var propertyGroupNodesDictionary = {};
            
            var scrollableNode = appRootNode.closest(".umb-scrollable");
            scrollableNode.addEventListener("scroll", onScroll);
            scrollableNode.addEventListener("mousewheel", cancelScrollTween);
            
            function onScroll(event) {
                
                var viewFocusY = scrollableNode.scrollTop + scrollableNode.clientHeight * .5;
                
                for(var i in $scope.content.tabs) {
                    var group = $scope.content.tabs[i];
                    var node = propertyGroupNodesDictionary[group.id];
                    if (viewFocusY >= node.offsetTop && viewFocusY <= node.offsetTop + node.clientHeight) {
                        setActiveAnchor(group);
                        return;
                    }
                }
                
            }
            
            function setActiveAnchor(tab) {
                if (tab.active !== true) {
                    var i = $scope.content.tabs.length;
                    while(i--) {
                        $scope.content.tabs[i].active = false;
                    }
                    tab.active = true;
                }
            }
            function getActiveAnchor() {
                var i = $scope.content.tabs.length;
                while(i--) {
                    if ($scope.content.tabs[i].active === true)
                        return $scope.content.tabs[i];
                }
                return false;
            }
            function getScrollPositionFor(id) {
                if (propertyGroupNodesDictionary[id]) {
                    return propertyGroupNodesDictionary[id].offsetTop - 20;// currently only relative to closest relatively positioned parent 
                }
                return null;
            }
            function scrollTo(id) {
                var y = getScrollPositionFor(id);
                if (getScrollPositionFor !== null) {
                    
                    var viewportHeight = scrollableNode.clientHeight;
                    var from = scrollableNode.scrollTop;
                    var to = Math.min(y, scrollableNode.scrollHeight - viewportHeight);
                    
                    var animeObject = {_y: from};
                    $scope.scrollTween = anime({
                        targets: animeObject,
                        _y: to,
                        easing: 'easeOutExpo',
                        duration: 200 + Math.min(Math.abs(to-from)/viewportHeight*100, 400),
                        update: () => {
                            scrollableNode.scrollTo(0, animeObject._y);
                        }
                    });

                }
            }
            function jumpTo(id) {
                var y = getScrollPositionFor(id);
                if (getScrollPositionFor !== null) {
                    cancelScrollTween();
                    scrollableNode.scrollTo(0, y);
                }
            }
            function cancelScrollTween() {
                if($scope.scrollTween) {
                    $scope.scrollTween.pause();
                }
            }
            
            $scope.registerPropertyGroup = function(element, appAnchor) {
                propertyGroupNodesDictionary[appAnchor] = element;
            };
            
            $scope.$on("editors.apps.appChanged", function($event, $args) {
                // if app changed to this app, then we want to scroll to the current anchor
                if($args.app.alias === "umbContent") {
                    var activeAnchor = getActiveAnchor();
                    $timeout(jumpTo.bind(null, [activeAnchor.id]));
                }
            });
            
            $scope.$on("editors.apps.appAnchorChanged", function($event, $args) {
                if($args.app.alias === "umbContent") {
                    setActiveAnchor($args.anchor);
                    scrollTo($args.anchor.id);
                }
            });
            
            //ensure to unregister from all dom-events
            $scope.$on('$destroy', function () {
                cancelScrollTween();
                scrollableNode.removeEventListener("scroll", onScroll);
                scrollableNode.removeEventListener("mousewheel", cancelScrollTween);
            });
            
        }

        function controller($scope) {
            
            //expose the property/methods for other directives to use
            this.content = $scope.content;
            
            if($scope.contentNodeModel) {
                $scope.defaultVariant = _.find($scope.contentNodeModel.variants, variant => {
                    // defaultVariant will never have segment. Wether it has a language or not depends on the setup.
                    return !variant.segment && ((variant.language && variant.language.isDefault) || (!variant.language));
                });
            }
            
            $scope.unlockInvariantValue = function(property) {
                property.unlockInvariantValue = !property.unlockInvariantValue;
            };

            $scope.$watch("tabbedContentForm.$dirty",
                function (newValue, oldValue) {
                    if (newValue === true) {
                        $scope.content.isDirty = true;
                    }
                }
            );

            $scope.propertyEditorDisabled = function (property) {
                if (property.unlockInvariantValue) {
                    return false;
                }
                
                var contentLanguage = $scope.content.language;

                var canEditCulture = !contentLanguage ||
                    // If the property culture equals the content culture it can be edited
                    property.culture === contentLanguage.culture ||
                    // A culture-invariant property can only be edited by the default language variant
                    (property.culture == null && contentLanguage.isDefault);                

                var canEditSegment = property.segment === $scope.content.segment;

                return !canEditCulture || !canEditSegment;
            }
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/content/umb-tabbed-content.html',
            controller: controller,
            link: link,
            scope: {
                content: "=", // in this context the content is the variant model.
                contentNodeModel: "=?" //contentNodeModel is the content model for the node, 
            }
        };

        return directive;

    }
    
    angular.module('umbraco.directives').directive('umbTabbedContent', tabbedContentDirective);

})();
