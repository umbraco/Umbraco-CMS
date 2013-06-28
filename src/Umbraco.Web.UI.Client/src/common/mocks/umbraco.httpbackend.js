var umbracoAppDev = angular.module('umbraco.mocks', ['umbraco', 'ngMockE2E']);

umbracoAppDev.run(function($httpBackend) {
  phones = [{name: 'phone1'}, {name: 'phone2'}];
 
  // returns the current list of phones
  $httpBackend.whenGET('/phones').respond(phones);
 
  // adds a new phone to the phones array
  $httpBackend.whenPOST('/phones').respond(function(method, url, data) {
    phones.push(angular.fromJSON(data));
  });
  
  $httpBackend.whenGET(/^\/templates\//).passThrough();
  //...
});