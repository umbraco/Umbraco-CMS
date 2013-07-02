describe('content type factory tests', function () {
    var $rootScope, $httpBackend, contentTypeResource, m;

   // beforeEach(module('ngMockE2E'));
    beforeEach(module('umbraco.resources'));
    beforeEach(module('umbraco.mocks'));
    
     beforeEach(inject(function ($injector) {
        $rootScope = $injector.get('$rootScope');
        $httpBackend = $injector.get('$httpBackend');
        mocks = $injector.get("contentTypeMocks");
        mocks.register();
        contentTypeResource = $injector.get('contentTypeResource');
    }));

    describe('global content type factory crud', function () {
        
        it('should return a content type object, given an id', function () {
            var ct1;
            contentTypeResource.getContentType(1234).then(function(result){
              ct1 = result;
            });

            $rootScope.$digest();
            
            expect(ct1).toNotBe(undefined);
            expect(ct1.id).toBe(1234);
        });


        it('should return a allowed content type collection given a document id', function(){

           // m.expectAllowedChildren();

            var collection;
            contentTypeResource.getAllowedTypes(1234).then(function(result){
                collection = result;
            });


            $rootScope.$digest();
            $httpBackend.flush();

            expect(collection.length).toBe(3);
        });      
  });
});