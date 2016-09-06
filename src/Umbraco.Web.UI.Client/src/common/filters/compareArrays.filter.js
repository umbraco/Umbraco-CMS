angular.module("umbraco.filters")
    .filter('compareArrays', function() {
        return function inArray(array, compareArray, compareProperty) {

            var result = [];

            angular.forEach(array, function(arrayItem){

                var exists = false;

                angular.forEach(compareArray, function(compareItem){
                    if( arrayItem[compareProperty] === compareItem[compareProperty]) {
                        exists = true;
                    }
                });

                if(!exists) {
                    result.push(arrayItem);
                }

            });

            return result;

        };
});
