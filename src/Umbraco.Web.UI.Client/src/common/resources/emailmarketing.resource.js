/**
 * @ngdoc service
 * @name umbraco.resources.emailMarketingResource
 * @description Used to add a backoffice user to Umbraco's email marketing system, if user opts in
 *
 *
 **/
function emailMarketingResource($http, umbRequestHelper) {

    // LOCAL
    // http://localhost:7071/api/EmailProxy

    // LIVE
    // https://emailcollector.umbraco.io/api/EmailProxy

    const emailApiUrl = 'https://emailcollector.umbraco.io/api/EmailProxy';

    //the factory object returned
    return {

        postAddUserToEmailMarketing: (user) => {
            return umbRequestHelper.resourcePromise(
                $http.post(emailApiUrl,
                {
                    name: user.name,
                    email: user.email,
                    usergroup: user.userGroups // [ "admin", "sensitiveData" ]
                }),
                'Failed to add user to email marketing list');
        }
    };
}

angular.module('umbraco.resources').factory('emailMarketingResource', emailMarketingResource);
