angular.module("umbraco.directives")
  .directive('selectOnFocus', function () {
    return function (scope, el, attrs) {
        $(el).on("click", function () {
            var editmode = $(el).data("editmode");
            //If editmode is true a click is handled like a normal click
            if (!editmode) {
                //Initial click, select entire text
                this.select();
                //Set the edit mode so subsequent clicks work normally
                $(el).data("editmode", true);
            }
        }).
        on("blur", function () {
            //Reset on focus lost
            $(el).data("editmode", false);
        });
    };
  });
