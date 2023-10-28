angular.module("umbraco.filters")
    .filter('compareArrays', function() {
        return function inArray(array, compareArray, compareProperty) {

            if (!compareArray || !compareArray.length) {
                return [...array];
            }

            var result = [];

            array.forEach(function(arrayItem){

                var exists = false;

                compareArray.forEach(function(compareItem){
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
