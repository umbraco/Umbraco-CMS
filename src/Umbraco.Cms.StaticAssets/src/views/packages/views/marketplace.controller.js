(function () {
  "use strict";

  function MarketplaceController($sce) {

    var vm = this;
    var marketplaceUrl = new URL(Umbraco.Sys.ServerVariables.umbracoUrls.marketplaceUrl);

    function init() {
      vm.marketplaceUrl = $sce.trustAsResourceUrl(marketplaceUrl.toString());
    }

    init();
  }

  angular.module("umbraco").controller("Umbraco.Editors.Packages.MarketplaceController", MarketplaceController);

})();
