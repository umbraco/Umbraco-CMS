describe('keyboard service tests', function () {
   var assetsService, $window, $rootScope;
   beforeEach(module('umbraco.services'));
   beforeEach(inject(function ($injector) {
        assetsService = $injector.get('assetsService');
        $window = $injector.get("$window");
        $rootScope = $injector.get('$rootScope');
   }));

   describe('Loading js assets', function () {
        
        it('Loads a javascript file', function () {
            assetsService.loadJs("lib/umbraco/NamespaceManager.js").then(function(){
                console.log("loaded");
            });
            
            //this currently doesnt work, the test server returns 404
            $rootScope.$digest();
        });
        

    });
});