describe('content type factory tests', function () {
    var $rootScope, contentTypeResource;

    beforeEach(module('umbraco.resources'));
    beforeEach(module('umbraco.httpbackend'));
    
    beforeEach(inject(function ($injector) {
      $rootScope = $injector.get('$rootScope');
      contentTypeResource = $injector.get('contentTypeResource');
    }));

    describe('global content type factory crud', function () {
        it('should return a content type object, given an id', function () {
            var ct1 = contentTypeResource.getContentType(1234);

            $rootScope.$digest();
            
            console.log("ct1:", ct1);
            
            expect(ct1).toNotBe(undefined);
            expect(ct1.id).toBe(1234);
        });



        it('should return a allowed content type collection given a document id', function(){
           //  var collection = contentTypeResource.getAllowedTypes(1234);
          //  $rootScope.$apply();

          //  console.log("running test", angular.toJson(collection));
          //  expect(collection.length).toBe(3);
        });      
  });
});