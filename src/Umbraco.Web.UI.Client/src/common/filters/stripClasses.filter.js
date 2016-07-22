angular.module("umbraco.filters").filter('stripClasses', function () {
    return function (str) {
        return str ? str.replace(/class=['"]([^"]*)["']/gi, '') : '';
    };
  });