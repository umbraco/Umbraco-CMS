describe('content factory tests', function () {
    var $scope, contentFactory;

    beforeEach(module('umbraco.mocks.resources'));

    beforeEach(inject(function($injector) {
      $scope = $injector.get('$rootScope');
      contentFactory = $injector.get('contentResource');
    }));

    describe('global content factory crud', function () {

        it('should return a content object, given an id', function () {
            var doc1 = contentFactory.getById(1234).then(function(doc1){
                expect(doc1).toNotBe(undefined);
                expect(doc1.id).toBe(1234);    
            });
        });

        it('should return a content children collection given an id', function () {
            var collection = contentFactory.getChildren(1234, undefined);
            expect(collection.resultSet.length).toBe(10);

            collection = contentFactory.getChildren(1234,{take: 5, offset: 1, filter: ""});
            expect(collection.resultSet.length).toBe(5);
        });      
  });
});