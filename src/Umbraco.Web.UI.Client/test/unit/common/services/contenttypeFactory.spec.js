describe('content type factory tests', function () {
    var $scope, contentTypeFactory;

    beforeEach(module('umbraco.mocks.resources'));

    beforeEach(inject(function($injector) {
      $scope = $injector.get('$rootScope');
      contentTypeFactory = $injector.get('contentTypeResource');
    }));

    describe('global content type factory crud', function () {

        it('should return a content type object, given an id', function () {
            var ct1 = contentTypeFactory.getContentType(1234);

            expect(ct1).toNotBe(undefined);
            expect(ct1.id).toBe(1234);
        });

        it('should return a allowed content type collection given a document id', function () {
            var collection = contentTypeFactory.getAllowedTypes(1234);
            expect(collection.length).toBe(3);
        });      
  });
});