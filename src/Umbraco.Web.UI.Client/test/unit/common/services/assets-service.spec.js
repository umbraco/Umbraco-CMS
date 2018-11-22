describe('Assets service tests', function () {
   var assetsService, $window, $rootScope;
   beforeEach(module('umbraco.services'));
   beforeEach(module('umbraco.mocks.services'));

   beforeEach(inject(function ($injector) {
        assetsService = $injector.get('assetsService');
        $window = $injector.get("$window");
        $rootScope = $injector.get('$rootScope');
   }));

   afterEach(inject(function($rootScope) {
     $rootScope.$apply();
   }));

   describe('Loading js assets', function () {
        
        it('Loads a javascript file', function () {

          var loaded = false;
          // runs( function(){
          //       assetsService.loadJs("lib/umbraco/NamespaceManager.js").then(function(){
          //           expect(Umbraco.Sys).toNotBe(undefined);
          //       });
          // });
          // runs(function(){
          //    expect(Umbraco.Sys).toNotBe(undefined);
          // });
        });
    });
});