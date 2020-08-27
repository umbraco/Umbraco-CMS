/**
 * @ngdoc service
 * @name umbraco.services.externalLoginInfoService
 * @description A service for working with external login providers
 **/
function externalLoginInfoService(externalLoginInfo, umbRequestHelper) {

    function getExternalLoginProvider(provider) {
        if (provider) {
            var found = _.find(externalLoginInfo.providers, x => x.authType == provider);
            return found;
        }
        return null;
    }

    function getExternalLoginProviderView(provider) {
        var found = getExternalLoginProvider(provider);
        if (found && found.properties.UmbracoBackOfficeExternalLoginOptions && found.properties.UmbracoBackOfficeExternalLoginOptions.BackOfficeCustomLoginView) {
            return umbRequestHelper.convertVirtualToAbsolutePath(found.properties.UmbracoBackOfficeExternalLoginOptions.BackOfficeCustomLoginView);
        }
        return null;
    }

    /**
     * Returns true if any provider denies local login if `provider` is null, else whether the passed 
     * @param {any} provider
     */
    function hasDenyLocalLogin(provider) {
        if (!provider) {
            return _.some(externalLoginInfo.providers, x => x.properties.UmbracoBackOfficeExternalLoginOptions.DenyLocalLogin === true);
        }
        else {
            return provider.properties.UmbracoBackOfficeExternalLoginOptions.DenyLocalLogin;
        }
    }

    /**
     * Returns all login providers
     * @param {any} excludeDenyLocalLogin true to exclude providers the deny local login
     */
    function getLoginProviders(excludeDenyLocalLogin) {
        if (excludeDenyLocalLogin) {
            return _.filter(externalLoginInfo.providers, x => !x.properties.UmbracoBackOfficeExternalLoginOptions.DenyLocalLogin);
        }
        else {
            return externalLoginInfo.providers;
        }
    }

    return {
        hasDenyLocalLogin: hasDenyLocalLogin,
        getLoginProviders: getLoginProviders,
        getExternalLoginProviderView: getExternalLoginProviderView
    };
}
angular.module('umbraco.services').factory('externalLoginInfoService', externalLoginInfoService);
