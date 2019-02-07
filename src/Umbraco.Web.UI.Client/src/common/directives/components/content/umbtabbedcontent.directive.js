(function () {
    'use strict';

    /** This directive is used to render out the current variant tabs and properties and exposes an API for other directives to consume  */
    function tabbedContentDirective() {

        function link($scope, $element, $attrs) {
            
            var appRootNode = $element[0];
            
            // Directive for cached property groups.
            var propertyGroupNodesDictionary = {};
            
            var scrollableNode = appRootNode.closest(".umb-scrollable");
            scrollableNode.addEventListener("scroll", onScroll);
            
            function onScroll(event) {
                
                var viewFocusY = scrollableNode.scrollTop + scrollableNode.clientHeight * .5;
                
                for(var i in $scope.content.tabs) {
                    var group = $scope.content.tabs[i];
                    var node = propertyGroupNodesDictionary[group.id];
                    //console.log(node.offsetTop, node.offsetTop + node.clientHeight)
                    if (viewFocusY >= node.offsetTop && viewFocusY <= node.offsetTop + node.clientHeight) {
                        setActiveAnchor(group);
                        return;
                    }
                }
                
            }
            function setActiveAnchor(tab) {
                var i = $scope.content.tabs.length;
                while(i--) {
                    $scope.content.tabs[i].active = false;
                }
                tab.active = true;
            }
            function scrollTo(id) {
                console.log("scrollTo", id);
                
                if (propertyGroupNodesDictionary[id]) {
                    let y = propertyGroupNodesDictionary[id].offsetTop - 20;// currently only relative to closest relatively positioned parent 
                    
                    scrollableNode.scrollTo(0, y);
                }
            }
            
            $scope.registerPropertyGroup = function(element, appAnchor) {
                propertyGroupNodesDictionary[appAnchor] = element;
            }
            
            $scope.$on("editors.apps.appAnchorChanged", function($event, $args) {
                if($args.app.alias === "umbContent") {
                    setActiveAnchor($args.anchor);
                    scrollTo($args.anchor.id);
                }
            });
            
            
        }

        function controller($scope, $element, $attrs) {
            
            
            //expose the property/methods for other directives to use
            this.content = $scope.content;
            this.activeVariant = _.find(this.content.variants, variant => {
                return variant.active;
            });

            $scope.activeVariant = this.activeVariant;

            $scope.defaultVariant = _.find(this.content.variants, variant => {
                return variant.language.isDefault;
            });

            $scope.unlockInvariantValue = function(property) {
                property.unlockInvariantValue = !property.unlockInvariantValue;
            };

            $scope.$watch("tabbedContentForm.$dirty",
                function (newValue, oldValue) {
                    if (newValue === true) {
                        $scope.content.isDirty = true;
                    }
                });
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/content/umb-tabbed-content.html',
            controller: controller,
            link: link,
            scope: {
                content: "="
            }
        };

        return directive;

    }
    
    angular.module('umbraco.directives').directive('umbTabbedContent', tabbedContentDirective);

})();
