angular.module("umbraco.directives")
  .directive('selectOnFocus', function () {
    return function (scope, el, attrs) {
        $(el).bind("click", function () {
            var editmode = $(el).data("editmode");
            if (editmode)
            {
                //Do nothing in this case
            }
            else {
                //initial click
                this.select();
                //Set the edit mode so subsequent clicks work normally
                $(el).data("editmode", true)
            }
        }).
        bind("blur", function () {
            //reset on blur
            $(el).data("editmode", false);
        });
    };
  });