(function () {
    "use strict";

    function umbGridHackScope() {
        
        function link($scope, $element) {
            
            // Since the grid used the el.scope() method, which should only be used by debugging, and only are avilable in debug-mode. I had to make a replica method doig the same:
            $element[0].getScope_HackForSortable = function() {
                return $scope;
            }
        }
        
        var directive = {
            restrict: "A",
            link: link
        };

        return directive;

    }
    
    angular.module("umbraco").directive("umbGridHackScope", umbGridHackScope);

})();
