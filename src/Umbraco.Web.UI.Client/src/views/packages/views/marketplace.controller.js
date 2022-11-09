(function () {
  "use strict";

  function MarketplaceController($sce) {

    var vm = this;
    var marketplaceUrl = new URL('https://dev.marketplace.umbraco.com');
    marketplaceUrl.searchParams.set('umbversion', Umbraco.Sys.ServerVariables.application.version);
    marketplaceUrl.searchParams.set('style', 'backoffice');

    function init() {
      vm.marketplaceUrl = $sce.trustAsResourceUrl(marketplaceUrl.toString());
    }

    init();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Packages.MarketplaceController", MarketplaceController);

})();
