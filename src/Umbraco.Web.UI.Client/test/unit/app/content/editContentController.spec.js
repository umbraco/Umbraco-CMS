describe('edit content controller tests', function () {
    var scope, controller, routeParams;
    routeParams = {id: 1234, create: false};

    beforeEach(module('umbraco'));

    beforeEach(inject(function($rootScope, $controller) {
      scope = $rootScope.$new();
      controller = $controller('Umbraco.Editors.ContentEditController', { 
                  $scope: scope, 
                  $routeParams : routeParams 
                }); 
    }));

    describe('content edit controller save and publish', function () {

      it('it should have an content object', function () {
        expect(scope.content).toNotBe(undefined);
        expect(scope.content.id).toBe(1234);
      });

      it('it should have a tabs collection', function () {
        expect(scope.content.tabs.length).toBe(5);
      });

      it('it should have a properties collection on each tab', function () {
            $(scope.content.tabs).each(function(i, tab){
                expect(tab.properties.length).toBeGreaterThan(0);
            });
      });

      it('it should change updateDate on save', function () {
        var currentUpdateDate = scope.content.updateDate;

        setTimeout(function(){
            scope.save(scope.content);
            expect(scope.content.updateDate).toBeGreaterThan(currentUpdateDate);
            }, 1000);
       });

      it('it should change publishDate on publish', function () {
        var currentPublishDate = scope.content.publishDate;

        //wait a sec before you publish
        setTimeout(function(){
            scope.saveAndPublish(scope.content);
            expect(scope.content.publishDate).toBeGreaterThan(currentPublishDate);
            }, 1000);
      });


    });
});