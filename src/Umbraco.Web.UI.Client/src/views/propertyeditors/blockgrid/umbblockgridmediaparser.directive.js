(function () {
    'use strict';

    function UmbMediaPropertyParser(mediaResource) {


        var directive = {
            restrict: 'A',
            scope: {
                input: '=umbMediaPropertyParserInput',
                output: '=umbMediaPropertyParserOutput'
            },
            link: function ($scope, $element, $attrs) {
                
                var unsubscribe = [];

                $scope.$watch($scope.input, function(newValue, oldValue) {
                    if (newValue !== oldValue) {
                        retrieveMedia(newValue);
                    }
                }, true);

                function retrieveMedia(inputValue) {

                    $scope.output = [];

                    if(inputValue && inputValue.length > 0) {
                        var keys = inputValue.map(x => x.mediaKey);

                        mediaResource.getById(keys[0]).then(function (mediaEntities) {
                            $scope.output = [mediaEntities];
                        });
                    }
                }
                
                retrieveMedia($scope.input);
        
                $scope.$on("$destroy", function () {
                    for (const subscription of unsubscribe) {
                        subscription();
                    }
                });
        

            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbMediaPropertyParserOutput', UmbMediaPropertyParser);

})();