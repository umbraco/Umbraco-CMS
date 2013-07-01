angular.module('umbraco.mocks').
  factory('mocksUtills', function () {
      'use strict';

      return {
          urlRegex: function(url) {
              url = url.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&");
              return new RegExp("^" + url);
          },

          getParameterByName: function(url, name) {
              name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
              var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                  results = regex.exec(url);
              return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
          }
      };
  });