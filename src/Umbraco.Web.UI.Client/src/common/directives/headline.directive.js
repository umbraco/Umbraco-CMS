angular.module("umbraco.directives")
  .directive('headline', function ($window) {
    return function (scope, el, attrs) {

      var h1 = $("<h1 class='umb-headline-editor'></h1>").hide();
      el.parent().prepend(h1);
      el.addClass("umb-headline-editor");

      if (el.val() !== '') {
        el.hide();
        h1.text(el.val());
        h1.show();
      } else {
        el.focus();
      }

      el.on("blur", function () {
        el.hide();
        h1.html(el.val()).show();
      });

      h1.on("click", function () {
        h1.hide();
        el.show().focus();
      });
    };
  });