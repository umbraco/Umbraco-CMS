/**
 * @ngdoc service
 * @name umbraco.services.externalLoginInfoService
 * @description A service for working with external login providers
 **/
function externalLoginInfoService(externalLoginInfo, umbRequestHelper) {

  function getLoginProvider(provider) {
    if (provider) {
      var found = _.find(externalLoginInfo.providers, x => x.authType == provider);
      return found;
    }
    return null;
  }

  function getLoginProviderView(provider) {
    if (provider && provider.properties && provider.properties.CustomBackOfficeView) {
      return umbRequestHelper.convertVirtualToAbsolutePath(provider.properties.CustomBackOfficeView);
    }
    return null;
  }

  /**
   * Returns true if any provider denies local login if `provider` is null, else whether the passed 
   * @param {any} provider
   */
  function hasDenyLocalLogin(provider) {
    if (!provider) {
      return _.some(externalLoginInfo.providers, x => x.properties && (x.properties.DenyLocalLogin === true));
    }
    else {
      return provider && provider.properties && (provider.properties.DenyLocalLogin === true);
    }
  }

  /**
   * Returns all login providers    
   */
  function getLoginProviders() {
    return externalLoginInfo.providers;
  }

  /** Returns all logins providers that have options that the user can interact with */
  function getLoginProvidersWithOptions() {
    // only include providers that allow manual linking or ones that provide a custom view
    var providers = _.filter(externalLoginInfo.providers, x => {
      // transform the data and also include the custom view as a nicer property
      x.customView = getLoginProviderView(x);
      if (x.customView) {
        return true;
      }
      else if (x.properties.AutoLinkOptions) {
        return x.properties.AutoLinkOptions.AllowManualLinking;
      }
      else {
        return false;
      }
    });
    return providers;
  }

  return {
    hasDenyLocalLogin: hasDenyLocalLogin,
    getLoginProvider: getLoginProvider,
    getLoginProviders: getLoginProviders,
    getLoginProvidersWithOptions: getLoginProvidersWithOptions,
    getLoginProviderView: getLoginProviderView
  };
}
angular.module('umbraco.services').factory('externalLoginInfoService', externalLoginInfoService);
