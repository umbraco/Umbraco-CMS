/**
 * @ngdoc service
 * @name umbraco.services.externalLoginInfoService
 * @description A service for working with external login providers
 **/
function externalLoginInfoService(externalLoginInfo) {

    /**
     * Returns true if any provider denies local login if `provider` is null, else whether the passed 
     * @param {any} provider
     */
    function hasDenyLocalLogin(provider) {
        if (!provider) {
            return _.some(externalLoginInfo.providers, x => x.properties.UmbracoBackOffice_DenyLocalLogin === true);
        }
        else {
            return provider.properties.UmbracoBackOffice_DenyLocalLogin;
        }
    }

    /**
     * Returns all login providers
     * @param {any} excludeDenyLocalLogin true to exclude providers the deny local login
     */
    function getLoginProviders(excludeDenyLocalLogin) {
        if (excludeDenyLocalLogin) {
            return _.filter(externalLoginInfo.providers, x => !x.properties.UmbracoBackOffice_DenyLocalLogin);
        }
        else {
            return externalLoginInfo.providers;
        }
    }

    return {
        hasDenyLocalLogin: hasDenyLocalLogin,
        getLoginProviders: getLoginProviders
    };
}
angular.module('umbraco.services').factory('externalLoginInfoService', externalLoginInfoService);
