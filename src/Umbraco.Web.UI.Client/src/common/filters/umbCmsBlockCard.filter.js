/**
 * @ngdoc filter
 * @name umbraco.filters.filter:umbCmsBlockCard
 * @namespace umbCmsBlockCard
 * 
 * @description
 * Filter block cards based on specific properties.
 * 
 */
angular.module("umbraco.filters").filter('umbCmsBlockCard', function () {
  return function (array, searchTerm) {
    // If no array is given, exit.
    if (!array) {
      return;
    }
    // If no search term exists, return the array unfiltered.
    else if (!searchTerm) {
      return array;
    }
    // Otherwise, continue.
    else {
      // Convert filter text to lower case.
      const term = searchTerm.toLowerCase();

      // Return the filtered array
      return array.filter((block, i) => {
        console.log("block", block);
        const props = ['id', 'key', 'udi', 'alias', 'name'];

        let found = false;

        for (let i = 0; i < props.length; i++) {
          console.log("prop", props[i]);
          console.log("id", block.elementTypeModel["id"]);
          if (!block.elementTypeModel.hasOwnProperty(props[i])) {
            continue;
          }

          if (block.elementTypeModel[props[i]].toString().toLowerCase().includes(term)) {
             found = true;
          }
        }

        return found;

      })
    }
  }
});
