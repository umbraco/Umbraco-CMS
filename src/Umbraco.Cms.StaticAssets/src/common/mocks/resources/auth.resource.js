/**
* @ngdoc service
* @name umbraco.mocks.authMocks
* @description
* Mocks data retrival for the auth service
**/
function authMocks($httpBackend, mocksUtils) {

    /** internal method to mock the current user to be returned */
    function getCurrentUser() {

        if (!mocksUtils.checkAuth()) {
            return [401, null, null];
        }

        var currentUser = {
            "email":"warren@umbraco.com",
            "locale":"en-US",
            "emailHash":"da0673cb2c930ee247e8ba5ebe4355bf",
            "userGroups":[
                "admin",
                "sensitiveData"
            ],
            "remainingAuthSeconds":1178.2645038,
            "startContentIds":[-1],
            "startMediaIds":[-1],
            "avatars":[
                "https://www.gravatar.com/avatar/da0673cb2c930ee247e8ba5ebe4355bf?d=404&s=30",
                "https://www.gravatar.com/avatar/da0673cb2c930ee247e8ba5ebe4355bf?d=404&s=60",
                "https://www.gravatar.com/avatar/da0673cb2c930ee247e8ba5ebe4355bf?d=404&s=90",
                "https://www.gravatar.com/avatar/da0673cb2c930ee247e8ba5ebe4355bf?d=404&s=150",
                "https://www.gravatar.com/avatar/da0673cb2c930ee247e8ba5ebe4355bf?d=404&s=300"
            ],
            "allowedSections":[
                "content",
                "forms",
                "media",
                "member",
                "packages",
                "settings",
                "users"
            ],
            "id":-1,
            "name":"Warren Buckley"
        };

        return [200, currentUser, null];
    }

    return {
        register: function () {
            $httpBackend
              .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Authentication/GetCurrentUser'))
              .respond(getCurrentUser);
        }
    };
}

angular.module('umbraco.mocks').factory('authMocks', ['$httpBackend', 'mocksUtils', authMocks]);
