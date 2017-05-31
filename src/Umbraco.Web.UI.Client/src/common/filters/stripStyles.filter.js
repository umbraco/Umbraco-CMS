angular.module("umbraco.filters").filter('stripStyles', function () {
    return function (str) {
        return str ? str.replace(/style=['"]([^"]*)["']/gi, '') : '';
    };
  });