(function () {
  "use strict";

  function MarketplaceController($sce) {

    var vm = this;
    var marketplaceUrl = 'https://dev.marketplace.umbraco.com';

    vm.onLoad = onLoad;
    vm.marketplaceUrl = $sce.trustAsResourceUrl(marketplaceUrl);

    function onLoad(evt) {
      // Listen for PostMessage from the iframe to know when it's loaded
      evt.target.addEventListener("message", onMessage);
    }

    function onMessage(event) {
      if (event.origin !== vm.marketplaceUrl) {
        return;
      }

      if (event.data === "loaded") {
        // Remove the listener once we know the iframe is loaded
        window.removeEventListener("message", onMessage);

        onMarketplaceLoaded();
      }
    }

    function onMarketplaceLoaded() {
      console.log('Marketplace is loaded - we need to do something here');
    }
  }

  angular.module("umbraco").controller("Umbraco.Editors.Packages.MarketplaceController", MarketplaceController);

})();
