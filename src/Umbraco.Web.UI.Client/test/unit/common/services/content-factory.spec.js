describe('content factory tests', function () {
    var $rootScope, $httpBackend, contentFactory, mocks, $http;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.resources'));
    beforeEach(module('umbraco.mocks'));

    beforeEach(inject(function ($injector, mocksUtils) {
        
        //for these tests we don't want any authorization to occur
        mocksUtils.disableAuth();

        $rootScope = $injector.get('$rootScope');
        $httpBackend = $injector.get('$httpBackend');
        mocks = $injector.get("contentMocks");
        mocks.register();
        contentFactory = $injector.get('contentResource');
    }));


    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });


    describe('global content factory crud', function () {
       
       
        it('should return a content object, given an id', function () {
            var doc;
            contentFactory.getById(1234).then(function(result){
                doc = result;
            });

            $rootScope.$digest();
            $httpBackend.flush();

            expect(doc).not.toBeUndefined();
            expect(doc.id).toBe(1234);   
        });


        it('should return a all children collection given an id', function () {

            var collection;
            contentFactory.getChildren(1234, undefined).then(function (data) {
                collection = data;
            });            
            $rootScope.$digest();
            $httpBackend.flush();            
            expect(collection.items.length).toBe(56);
        });
        
        it('should return paged children collection given an id', function () {

            var collection;
            contentFactory.getChildren(1234, { pageSize: 5, pageNumber: 1, filter: "" }).then(function (data) {
                collection = data;
            });
            $rootScope.$digest();
            $httpBackend.flush();
            expect(collection.items.length).toBe(5);
        });
  });
});