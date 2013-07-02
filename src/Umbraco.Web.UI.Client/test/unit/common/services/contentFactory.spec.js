describe('content factory tests', function () {
    var $rootScope, $httpBackend, contentFactory, mocks, $http;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.resources'));
    beforeEach(module('umbraco.httpbackend'));

    beforeEach(inject(function ($injector) {
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

            expect(doc).toNotBe(undefined);
            expect(doc.id).toBe(1234);   
        });


        it('should return a content children collection given an id', function () {
            var collection = contentFactory.getChildren(1234, undefined);
            expect(collection.resultSet.length).toBe(10);

            collection = contentFactory.getChildren(1234,{take: 5, offset: 1, filter: ""});
            expect(collection.resultSet.length).toBe(5);
        });      
  });
});