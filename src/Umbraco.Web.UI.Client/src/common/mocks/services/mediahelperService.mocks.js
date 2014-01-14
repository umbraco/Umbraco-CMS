angular.module('umbraco.mocks').
  factory('imageHelperMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
      'use strict';
      
      function returnEntitybyIds(){
        return "hello.jpg";
      }


      return {
          register: function () {

              $httpBackend
                  .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Images/GetBigThumbnail'))
                  .respond(returnEntitybyIds);

              
          }
      };
  }]);