(function () {
  "use strict";

  function MarketplaceController($sce) {

    var vm = this;
    var marketplaceUrl = 'https://dev.marketplace.umbraco.com';

    vm.onLoad = onLoad;
    vm.marketplaceUrl = $sce.trustAsResourceUrl(marketplaceUrl);
  }

  angular.module("umbraco").controller("Umbraco.Editors.Packages.MarketplaceController", MarketplaceController);

})();
