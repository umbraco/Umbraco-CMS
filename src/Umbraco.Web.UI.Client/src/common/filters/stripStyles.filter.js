angular.module("umbraco.filters").filter('stripStyles', function () {
    return function (str) {
        console.log("str", str);
        return str ? str.replace(/style=['"]([^"]*)["']/gi, '') : '';
        //return str ? str.replace(/style=['"].*["']/, '') : '';
    };
  });