(function () {
    'use strict';
    angular.module('umbraco.resources', []);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.authResource
 * @description
 * This Resource perfomrs actions to common authentication tasks for the Umbraco backoffice user
 *
 * @requires $q 
 * @requires $http
 * @requires umbRequestHelper
 * @requires angularHelper
 */
    function authResource($q, $http, umbRequestHelper, angularHelper) {
        return {
            /**
    * @ngdoc method
    * @name umbraco.resources.authResource#get2FAProviders
    * @methodOf umbraco.resources.authResource
    *
    * @description
    * Logs the Umbraco backoffice user in if the credentials are good
    *
    * ##usage
    * <pre>
    * authResource.get2FAProviders()
    *    .then(function(data) {
    *        //Do stuff ...
    *    });
    * </pre>
    * @returns {Promise} resourcePromise object
    * 
    */
            get2FAProviders: function get2FAProviders() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'Get2FAProviders')), 'Could not retrive two factor provider info');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.authResource#get2FAProviders
    * @methodOf umbraco.resources.authResource
    *
    * @description
    * Generate the two-factor authentication code for the provider and send it to the user
    *
    * ##usage
    * <pre>
    * authResource.send2FACode(provider)
    *    .then(function(data) {
    *        //Do stuff ...
    *    });
    * </pre>
    * @param {string} provider Name of the provider
    * @returns {Promise} resourcePromise object
    *
    */
            send2FACode: function send2FACode(provider) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'PostSend2FACode'), Utilities.toJson(provider)), 'Could not send code');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.authResource#get2FAProviders
    * @methodOf umbraco.resources.authResource
    *
    * @description
    * Verify the two-factor authentication code entered by the user against the provider
    *
    * ##usage
    * <pre>
    * authResource.verify2FACode(provider, code)
    *    .then(function(data) {
    *        //Do stuff ...
    *    });
    * </pre>
    * @param {string} provider Name of the provider
    * @param {string} code The two-factor authentication code
    * @returns {Promise} resourcePromise object
    *
    */
            verify2FACode: function verify2FACode(provider, code) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'PostVerify2FACode'), {
                    code: code,
                    provider: provider
                }), 'Could not verify code');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.authResource#performLogin
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Logs the Umbraco backoffice user in if the credentials are good
     *
     * ##usage
     * <pre>
     * authResource.performLogin(login, password)
     *    .then(function(data) {
     *        //Do stuff for login...
     *    });
     * </pre> 
     * @param {string} login Username of backoffice user
     * @param {string} password Password of backoffice user
     * @returns {Promise} resourcePromise object
     *
     */
            performLogin: function performLogin(username, password) {
                if (!username || !password) {
                    return $q.reject({ errorMsg: 'Username or password cannot be empty' });
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'PostLogin'), {
                    username: username,
                    password: password
                }), 'Login failed for user ' + username);
            },
            /**
     * There are not parameters for this since when the user has clicked on their invite email they will be partially
     * logged in (but they will not be approved) so we need to use this method to verify the non approved logged in user's details.
     * Using the getCurrentUser will not work since that only works for approved users
     * @returns {} 
     */
            getCurrentInvitedUser: function getCurrentInvitedUser() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'GetCurrentInvitedUser')), 'Failed to verify invite');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.authResource#performRequestPasswordReset
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Checks to see if the provided email address is a valid user account and sends a link
     * to allow them to reset their password
     *
     * ##usage
     * <pre>
     * authResource.performRequestPasswordReset(email)
     *    .then(function(data) {
     *        //Do stuff for password reset request...
     *    });
     * </pre> 
     * @param {string} email Email address of backoffice user
     * @returns {Promise} resourcePromise object
     *
     */
            performRequestPasswordReset: function performRequestPasswordReset(email) {
                if (!email) {
                    return $q.reject({ errorMsg: 'Email address cannot be empty' });
                }
                // TODO: This validation shouldn't really be done here, the validation on the login dialog
                // is pretty hacky which is why this is here, ideally validation on the login dialog would
                // be done properly.
                var emailRegex = /\S+@\S+\.\S+/;
                if (!emailRegex.test(email)) {
                    return $q.reject({ errorMsg: 'Email address is not valid' });
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'PostRequestPasswordReset'), { email: email }), 'Request password reset failed for email ' + email);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.authResource#performValidatePasswordResetCode
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Checks to see if the provided password reset code is valid
     *
     * ##usage
     * <pre>
     * authResource.performValidatePasswordResetCode(resetCode)
     *    .then(function(data) {
     *        //Allow reset of password
     *    });
     * </pre> 
     * @param {integer} userId User Id
     * @param {string} resetCode Password reset code
     * @returns {Promise} resourcePromise object
     *
     */
            performValidatePasswordResetCode: function performValidatePasswordResetCode(userId, resetCode) {
                if (!userId) {
                    return $q.reject({ errorMsg: 'User Id cannot be empty' });
                }
                if (!resetCode) {
                    return $q.reject({ errorMsg: 'Reset code cannot be empty' });
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'PostValidatePasswordResetCode'), {
                    userId: userId,
                    resetCode: resetCode
                }), 'Password reset code validation failed for userId ' + userId + ', code' + resetCode);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.currentUserResource#getMembershipProviderConfig
     * @methodOf umbraco.resources.currentUserResource
     *
     * @description
     * Gets the configuration of the user membership provider which is used to configure the change password form         
     */
            getMembershipProviderConfig: function getMembershipProviderConfig() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'GetMembershipProviderConfig')), 'Failed to retrieve membership provider config');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.authResource#performSetPassword
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Checks to see if the provided password reset code is valid and sets the user's password
     *
     * ##usage
     * <pre>
     * authResource.performSetPassword(userId, password, confirmPassword, resetCode)
     *    .then(function(data) {
     *        //Password set
     *    });
     * </pre> 
     * @param {integer} userId User Id
     * @param {string} password New password
     * @param {string} confirmPassword Confirmation of new password
     * @param {string} resetCode Password reset code
     * @returns {Promise} resourcePromise object
     *
     */
            performSetPassword: function performSetPassword(userId, password, confirmPassword, resetCode) {
                if (userId === undefined || userId === null) {
                    return $q.reject({ errorMsg: 'User Id cannot be empty' });
                }
                if (!password) {
                    return $q.reject({ errorMsg: 'Password cannot be empty' });
                }
                if (password !== confirmPassword) {
                    return $q.reject({ errorMsg: 'Password and confirmation do not match' });
                }
                if (!resetCode) {
                    return $q.reject({ errorMsg: 'Reset code cannot be empty' });
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'PostSetPassword'), {
                    userId: userId,
                    password: password,
                    resetCode: resetCode
                }), 'Password reset code validation failed for userId ' + userId);
            },
            unlinkLogin: function unlinkLogin(loginProvider, providerKey) {
                if (!loginProvider || !providerKey) {
                    return $q.reject({ errorMsg: 'loginProvider or providerKey cannot be empty' });
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'PostUnLinkLogin'), {
                    loginProvider: loginProvider,
                    providerKey: providerKey
                }), 'Unlinking login provider failed');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.authResource#performLogout
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Logs out the Umbraco backoffice user
     *
     * ##usage
     * <pre>
     * authResource.performLogout()
     *    .then(function(data) {
     *        //Do stuff for logging out...
     *    });
     * </pre>
     * @returns {Promise} resourcePromise object
     *
     */
            performLogout: function performLogout() {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'PostLogout')));
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.authResource#getCurrentUser
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Sends a request to the server to get the current user details, will return a 401 if the user is not logged in
     *
     * ##usage
     * <pre>
     * authResource.getCurrentUser()
     *    .then(function(data) {
     *        //Do stuff for fetching the current logged in Umbraco backoffice user
     *    });
     * </pre>
     * @returns {Promise} resourcePromise object
     *
     */
            getCurrentUser: function getCurrentUser() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'GetCurrentUser')), 'Server call failed for getting current user');
            },
            getCurrentUserLinkedLogins: function getCurrentUserLinkedLogins() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'GetCurrentUserLinkedLogins')), 'Server call failed for getting current users linked logins');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.authResource#isAuthenticated
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Checks if the user is logged in or not - does not return 401 or 403
     *
     * ##usage
     * <pre>
     * authResource.isAuthenticated()
     *    .then(function(data) {
     *        //Do stuff to check if user is authenticated
     *    });
     * </pre>
     * @returns {Promise} resourcePromise object
     *
     */
            isAuthenticated: function isAuthenticated() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'IsAuthenticated')), {
                    success: function success(data, status, headers, config) {
                        //if the response is false, they are not logged in so return a rejection
                        if (data === false || data === 'false') {
                            return $q.reject('User is not logged in');
                        }
                        return data;
                    },
                    error: function error(data, status, headers, config) {
                        return {
                            errorMsg: 'Server call failed for checking authentication',
                            data: data,
                            status: status
                        };
                    }
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.authResource#getRemainingTimeoutSeconds
     * @methodOf umbraco.resources.authResource
     *
     * @description
     * Gets the user's remaining seconds before their login times out
     *
     * ##usage
     * <pre>
     * authResource.getRemainingTimeoutSeconds()
     *    .then(function(data) {
     *        //Number of seconds is returned
     *    });
     * </pre>
     * @returns {Promise} resourcePromise object
     *
     */
            getRemainingTimeoutSeconds: function getRemainingTimeoutSeconds() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('authenticationApiBaseUrl', 'GetRemainingTimeoutSeconds')), 'Server call failed for checking remaining seconds');
            }
        };
    }
    angular.module('umbraco.resources').factory('authResource', authResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.codefileResource
    * @description Loads in data for files that contain code such as js scripts, partial views and partial view macros
    **/
    function codefileResource($q, $http, umbDataFormatter, umbRequestHelper, localizationService) {
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#getByPath
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Gets a codefile item with a given path
     *
     * ##usage
     * <pre>
     * codefileResource.getByPath('scripts', 'oooh-la-la.js')
     *    .then(function(codefile) {
     *        alert('its here!');
     *    });
     * </pre>
     * 
     * <pre>
     * codefileResource.getByPath('partialView', 'Grid%2fEditors%2fBase.cshtml')
     *    .then(function(codefile) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {type} the type of script (partialView, partialViewMacro, script)
     * @param {virtualpath} the virtual path of the script
     * @returns {Promise} resourcePromise object.
     *
     */
            getByPath: function getByPath(type, virtualpath) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('codeFileApiBaseUrl', 'GetByPath', [
                    { type: type },
                    { virtualPath: virtualpath }
                ])), 'Failed to retrieve data for ' + type + ' from virtual path ' + virtualpath);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#getByAlias
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Gets a template item with a given alias
     *
     * ##usage
     * <pre>
     * codefileResource.getByAlias("upload")
     *    .then(function(template) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {String} alias Alias of template to retrieve
     * @returns {Promise} resourcePromise object.
     *
     */
            getByAlias: function getByAlias(alias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('templateApiBaseUrl', 'GetByAlias', [{ alias: alias }])), 'Failed to retrieve data for template with alias: ' + alias);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#deleteByPath
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Deletes a codefile with a given type & path
     *
     * ##usage
     * <pre>
     * codefileResource.deleteByPath('scripts', 'oooh-la-la.js')
     *    .then(function() {
     *        alert('its gone!');
     *    });
     * </pre>
     * 
     * <pre>
     * codefileResource.deleteByPath('partialViews', 'Grid%2fEditors%2fBase.cshtml')
     *    .then(function() {
     *        alert('its gone!');
     *    });
     * </pre>
     *
     * @param {type} the type of script (partialViews, partialViewMacros, scripts)
     * @param {virtualpath} the virtual path of the script
     * @returns {Promise} resourcePromise object.
     *
     */
            deleteByPath: function deleteByPath(type, virtualpath) {
                var promise = localizationService.localize('codefile_deleteItemFailed', [virtualpath]);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('codeFileApiBaseUrl', 'Delete', [
                    { type: type },
                    { virtualPath: virtualpath }
                ])), promise);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#save
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Saves or update a codeFile
     * 
     * ##usage
     * <pre>
     * codefileResource.save(codeFile)
     *    .then(function(codeFile) {
     *        alert('its saved!');
     *    });
     * </pre>
     *
     * @param {Object} template object to save
     * @returns {Promise} resourcePromise object.
     *
     */
            save: function save(codeFile) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('codeFileApiBaseUrl', 'PostSave'), codeFile), 'Failed to save data for code file ' + codeFile.virtualPath);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#getSnippets
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Gets code snippets for a given file type
     * 
     * ##usage
     * <pre>
     * codefileResource.getSnippets("partialViews")
     *    .then(function(snippets) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {string} file type: (partialViews, partialViewMacros)
     * @returns {Promise} resourcePromise object.
     *
     */
            getSnippets: function getSnippets(fileType) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('codeFileApiBaseUrl', 'GetSnippets?type=' + fileType)), 'Failed to get snippet for' + fileType);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#getScaffold
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Returns a scaffold of an empty codefile item.
     * 
     * The scaffold is used to build editors for code file editors that has not yet been populated with data.
     * 
     * ##usage
     * <pre>
     * codefileResource.getScaffold("partialViews", "Breadcrumb")
     *    .then(function(data) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {string} File type: (scripts, partialViews, partialViewMacros).
     * @param {string} Snippet name (Ex. Breadcrumb).
     * @returns {Promise} resourcePromise object.
     *
     */
            getScaffold: function getScaffold(type, id, snippetName) {
                var queryString = '?type=' + type + '&id=' + id;
                if (snippetName) {
                    queryString += '&snippetName=' + snippetName;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('codeFileApiBaseUrl', 'GetScaffold' + queryString)), 'Failed to get scaffold for' + type);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#createContainer
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Creates a container/folder
     * 
     * ##usage
     * <pre>
     * codefileResource.createContainer("partialViews", "folder%2ffolder", "folder")
     *    .then(function(data) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {string} File type: (scripts, partialViews, partialViewMacros).
     * @param {string} Parent Id: url encoded path
     * @param {string} Container name
     * @returns {Promise} resourcePromise object.
     *
     */
            createContainer: function createContainer(type, parentId, name) {
                // Is the parent ID numeric?
                var key = 'codefile_createFolderFailedBy' + (isNaN(parseInt(parentId)) ? 'Name' : 'Id');
                var promise = localizationService.localize(key, [parentId]);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('codeFileApiBaseUrl', 'PostCreateContainer', {
                    type: type,
                    parentId: parentId,
                    name: encodeURIComponent(name)
                })), promise);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#interpolateStylesheetRules
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Takes all rich text editor styling rules and turns them into css
     * 
     * ##usage
     * <pre>
     * codefileResource.interpolateStylesheetRules(".box{background:purple;}", "[{name: "heading", selector: "h1", styles: "color: red"}]")
     *    .then(function(data) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {string} content The style sheet content.
     * @param {string} rules The rich text editor rules
     * @returns {Promise} resourcePromise object.
     *
     */
            interpolateStylesheetRules: function interpolateStylesheetRules(content, rules) {
                var payload = {
                    content: content,
                    rules: rules
                };
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('codeFileApiBaseUrl', 'PostInterpolateStylesheetRules'), payload), 'Failed to interpolate sheet rules');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.codefileResource#extractStylesheetRules
     * @methodOf umbraco.resources.codefileResource
     *
     * @description
     * Find all rich text editor styles in the style sheets and turns them into "rules"
     * 
     * ##usage
     * <pre>
     * 
     * var conntent
     * codefileResource.extractStylesheetRules(".box{background:purple;}")
     *    .then(function(data) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {string} content The style sheet content.
     * @returns {Promise} resourcePromise object.
     *
     */
            extractStylesheetRules: function extractStylesheetRules(content) {
                var payload = {
                    content: content,
                    rules: null
                };
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('codeFileApiBaseUrl', 'PostExtractStylesheetRules'), payload), 'Failed to extract style sheet rules');
            }
        };
    }
    angular.module('umbraco.resources').factory('codefileResource', codefileResource);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.contentResource
 * @description Handles all transactions of content data
 * from the angular application to the Umbraco database, using the Content WebApi controller
 *
 * all methods returns a resource promise async, so all operations won't complete untill .then() is completed.
 *
 * @requires $q
 * @requires $http
 * @requires umbDataFormatter
 * @requires umbRequestHelper
 *
 * ##usage
 * To use, simply inject the contentResource into any controller or service that needs it, and make
 * sure the umbraco.resources module is accesible - which it should be by default.
 *
 * <pre>
 *    contentResource.getById(1234)
 *          .then(function(data) {
 *              $scope.content = data;
  *          });
  * </pre>
 **/
    function contentResource($q, $http, umbDataFormatter, umbRequestHelper) {
        /** internal method process the saving of data and post processing the result */
        function saveContentItem(content, action, files, restApiUrl, showNotifications) {
            return umbRequestHelper.postSaveContent({
                restApiUrl: restApiUrl,
                content: content,
                action: action,
                files: files,
                showNotifications: showNotifications,
                dataFormatter: function dataFormatter(c, a) {
                    return umbDataFormatter.formatContentPostData(c, a);
                }
            });
        }
        return {
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#allowsCultureVariation
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Check whether any content types have culture variant enabled
    *
    * ##usage
    * <pre>
    * contentResource.allowsCultureVariation()
    *    .then(function() {
    *       Do stuff...
    *    });
    * </pre>
    * 
    * @returns {Promise} resourcePromise object.
    *
    */
            allowsCultureVariation: function allowsCultureVariation() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'AllowsCultureVariation')), 'Failed to retrieve variant content types');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#savePermissions
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Save user group permissions for the content
    *
    * ##usage
    * <pre>
    * contentResource.savePermissions(saveModel)
    *    .then(function() {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {object} The object which contains the user group permissions for the content
    * @returns {Promise} resourcePromise object.
    *
    */
            savePermissions: function savePermissions(saveModel) {
                if (!saveModel) {
                    throw 'saveModel cannot be null';
                }
                if (!saveModel.contentId) {
                    throw 'saveModel.contentId cannot be null';
                }
                if (!saveModel.permissions) {
                    throw 'saveModel.permissions cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSaveUserGroupPermissions'), saveModel), 'Failed to save permissions');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#getRecycleBin
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Get the recycle bin
    *
    * ##usage
    * <pre>
    * contentResource.getRecycleBin()
    *    .then(function() {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @returns {Promise} resourcePromise object.
    *
    */
            getRecycleBin: function getRecycleBin() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetRecycleBin')), 'Failed to retrieve data for content recycle bin');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#sort
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Sorts all children below a given parent node id, based on a collection of node-ids
     *
     * ##usage
     * <pre>
     * var ids = [123,34533,2334,23434];
     * contentResource.sort({ parentId: 1244, sortedIds: ids })
     *    .then(function() {
     *        $scope.complete = true;
     *    });
      * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.parentId the ID of the parent node
     * @param {Array} options.sortedIds array of node IDs as they should be sorted
     * @returns {Promise} resourcePromise object.
     *
     */
            sort: function sort(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.sortedIds) {
                    throw 'args.sortedIds cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSort'), {
                    parentId: args.parentId,
                    idSortOrder: args.sortedIds
                }), 'Failed to sort content');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#move
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Moves a node underneath a new parentId
     *
     * ##usage
     * <pre>
     * contentResource.move({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("node was moved");
     *    }, function(err){
      *      alert("node didnt move:" + err.data.Message);
     *    });
      * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.idd the ID of the node to move
     * @param {Int} args.parentId the ID of the parent node to move to
     * @returns {Promise} resourcePromise object.
     *
     */
            move: function move(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostMove'), {
                    parentId: args.parentId,
                    id: args.id
                }, { responseType: 'text' }), 'Failed to move content');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#copy
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Copies a node underneath a new parentId
     *
     * ##usage
     * <pre>
     * contentResource.copy({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("node was copied");
     *    }, function(err){
      *      alert("node wasnt copy:" + err.data.Message);
     *    });
      * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.id the ID of the node to copy
     * @param {Int} args.parentId the ID of the parent node to copy to
     * @param {Boolean} args.relateToOriginal if true, relates the copy to the original through the relation api
     * @returns {Promise} resourcePromise object.
     *
     */
            copy: function copy(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostCopy'), args, { responseType: 'text' }), 'Failed to copy content');
            },
            /**
     * @ngdoc method
      * @name umbraco.resources.contentResource#unpublish
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Unpublishes a content item with a given Id
     *
     * ##usage
     * <pre>
      * contentResource.unpublish(1234)
     *    .then(function() {
     *        alert("node was unpulished");
     *    }, function(err){
      *      alert("node wasnt unpublished:" + err.data.Message);
     *    });
      * </pre>
     * @param {Int} id the ID of the node to unpublish
     * @returns {Promise} resourcePromise object.
     *
     */
            unpublish: function unpublish(id, cultures) {
                if (!id) {
                    throw 'id cannot be null';
                }
                if (!cultures) {
                    cultures = [];
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostUnpublish'), {
                    id: id,
                    cultures: cultures
                }), 'Failed to publish content with id ' + id);
            },
            /**
     * @ngdoc method
      * @name umbraco.resources.contentResource#getCultureAndDomains
      * @methodOf umbraco.resources.contentResource
      *
      * @description
      * Gets the culture and hostnames for a content item with the given Id
      *
      * ##usage
      * <pre>
      * contentResource.getCultureAndDomains(1234)
      *    .then(function(data) {
      *        alert(data.Domains, data.Language);
      *    });
      * </pre>
      * @param {Int} id the ID of the node to get the culture and domains for.
      * @returns {Promise} resourcePromise object.
      *
      */
            getCultureAndDomains: function getCultureAndDomains(id) {
                if (!id) {
                    throw 'id cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetCultureAndDomains', { id: id })), 'Failed to retreive culture and hostnames for ' + id);
            },
            saveLanguageAndDomains: function saveLanguageAndDomains(model) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSaveLanguageAndDomains'), model));
            },
            /**
      * @ngdoc method
     * @name umbraco.resources.contentResource#emptyRecycleBin
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Empties the content recycle bin
     *
     * ##usage
     * <pre>
     * contentResource.emptyRecycleBin()
     *    .then(function() {
     *        alert('its empty!');
     *    });
      * </pre>
      *
     * @returns {Promise} resourcePromise object.
     *
     */
            emptyRecycleBin: function emptyRecycleBin() {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'EmptyRecycleBin')), 'Failed to empty the recycle bin');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#deleteById
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Deletes a content item with a given id
     *
     * ##usage
     * <pre>
     * contentResource.deleteById(1234)
     *    .then(function() {
     *        alert('its gone!');
     *    });
      * </pre>
      *
      * @param {Int} id id of content item to delete
     * @returns {Promise} resourcePromise object.
     *
     */
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete item ' + id);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#deleteBlueprint
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Deletes a content blueprint item with a given id
    *
    * ##usage
    * <pre>
    * contentResource.deleteBlueprint(1234)
    *    .then(function() {
    *        alert('its gone!');
    *    });
    * </pre>
    *
    * @param {Int} id id of content blueprint item to delete
    * @returns {Promise} resourcePromise object.
    *
    */
            deleteBlueprint: function deleteBlueprint(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'DeleteBlueprint', [{ id: id }])), 'Failed to delete blueprint ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#getById
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Gets a content item with a given id
     *
     * ##usage
     * <pre>
     * contentResource.getById(1234)
     *    .then(function(content) {
      *        var myDoc = content;
     *        alert('its here!');
     *    });
      * </pre>
      *
      * @param {Int} id id of content item to return
      * @param {Int} culture optional culture to retrieve the item in
     * @returns {Promise} resourcePromise object containing the content item.
     *
     */
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetById', { id: id })), 'Failed to retrieve data for content id ' + id).then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#getBlueprintById
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Gets a content blueprint item with a given id
    *
    * ##usage
    * <pre>
    * contentResource.getBlueprintById(1234)
    *    .then(function() {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {Int} id id of content blueprint item to retrieve
    * @returns {Promise} resourcePromise object.
    *
    */
            getBlueprintById: function getBlueprintById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetBlueprintById', { id: id })), 'Failed to retrieve data for content id ' + id).then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#getNotifySettingsById
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Gets notification options for a content item with a given id for the current user
    *
    * ##usage
    * <pre>
    * contentResource.getNotifySettingsById(1234)
    *    .then(function() {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {Int} id id of content item
    * @returns {Promise} resourcePromise object.
    *
    */
            getNotifySettingsById: function getNotifySettingsById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetNotificationOptions', { contentId: id })), 'Failed to retrieve data for content id ' + id);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#getNotifySettingsById
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Sets notification settings for a content item with a given id for the current user
    *
    * ##usage
    * <pre>
    * contentResource.setNotifySettingsById(1234,["D", "F", "H"])
    *    .then(function() {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {Int} id id of content item
    * @param {Array} options the notification options to set for the content item
    * @returns {Promise} resourcePromise object.
    *
    */
            setNotifySettingsById: function setNotifySettingsById(id, options) {
                if (!id) {
                    throw 'contentId cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostNotificationOptions', {
                    contentId: id,
                    notifyOptions: options
                })), 'Failed to set notify settings for content id ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#getByIds
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Gets an array of content items, given a collection of ids
     *
     * ##usage
     * <pre>
     * contentResource.getByIds( [1234,2526,28262])
     *    .then(function(contentArray) {
      *        var myDoc = contentArray;
     *        alert('they are here!');
     *    });
      * </pre>
      *
      * @param {Array} ids ids of content items to return as an array
     * @returns {Promise} resourcePromise object containing the content items array.
     *
     */
            getByIds: function getByIds(ids) {
                var idQuery = '';
                ids.forEach(function (id) {
                    return idQuery += 'ids='.concat(id, '&');
                });
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetByIds', idQuery)), 'Failed to retrieve data for content with multiple ids').then(function (result) {
                    //each item needs to be re-formatted
                    result.forEach(function (r) {
                        return umbDataFormatter.formatContentGetData(r);
                    });
                    return $q.when(result);
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#getScaffold
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Returns a scaffold of an empty content item, given the id of the content item to place it underneath and the content type alias.
      *
     * - Parent Id must be provided so umbraco knows where to store the content
      * - Content Type alias must be provided so umbraco knows which properties to put on the content scaffold
      *
     * The scaffold is used to build editors for content that has not yet been populated with data.
      *
     * ##usage
     * <pre>
     * contentResource.getScaffold(1234, 'homepage')
     *    .then(function(scaffold) {
     *        var myDoc = scaffold;
      *        myDoc.name = "My new document";
     *
     *        contentResource.publish(myDoc, true)
     *            .then(function(content){
     *                alert("Retrieved, updated and published again");
     *            });
     *    });
      * </pre>
      *
     * @param {Int} parentId id of content item to return
      * @param {String} alias contenttype alias to base the scaffold on
     * @returns {Promise} resourcePromise object containing the content scaffold.
     *
     */
            getScaffold: function getScaffold(parentId, alias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetEmpty', {
                    contentTypeAlias: alias,
                    parentId: parentId
                })), 'Failed to retrieve data for empty content item type ' + alias).then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#getScaffoldByKey
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Returns a scaffold of an empty content item, given the id of the content item to place it underneath and the content type alias.
      *
     * - Parent Id must be provided so umbraco knows where to store the content
      * - Content Type Id must be provided so umbraco knows which properties to put on the content scaffold
      *
     * The scaffold is used to build editors for content that has not yet been populated with data.
      *
     * ##usage
     * <pre>
     * contentResource.getScaffoldByKey(1234, '...')
     *    .then(function(scaffold) {
     *        var myDoc = scaffold;
      *        myDoc.name = "My new document";
     *
     *        contentResource.publish(myDoc, true)
     *            .then(function(content){
     *                alert("Retrieved, updated and published again");
     *            });
     *    });
      * </pre>
      *
     * @param {Int} parentId id of content item to return
      * @param {String} contentTypeGuid contenttype guid to base the scaffold on
     * @returns {Promise} resourcePromise object containing the content scaffold.
     *
     */
            getScaffoldByKey: function getScaffoldByKey(parentId, contentTypeKey) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetEmptyByKey', {
                    contentTypeKey: contentTypeKey,
                    parentId: parentId
                })), 'Failed to retrieve data for empty content item id ' + contentTypeKey).then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
            },
            getBlueprintScaffold: function getBlueprintScaffold(parentId, blueprintId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetEmpty', {
                    blueprintId: blueprintId,
                    parentId: parentId
                })), 'Failed to retrieve blueprint for id ' + blueprintId).then(function (result) {
                    return $q.when(umbDataFormatter.formatContentGetData(result));
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#getNiceUrl
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Returns a url, given a node ID
     *
     * ##usage
     * <pre>
     * contentResource.getNiceUrl(id)
     *    .then(function(url) {
     *        alert('its here!');
     *    });
      * </pre>
      *
     * @param {Int} id Id of node to return the public url to
     * @returns {Promise} resourcePromise object containing the url.
     *
     */
            getNiceUrl: function getNiceUrl(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetNiceUrl', { id: id }), { responseType: 'text' }), 'Failed to retrieve url for id:' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#getChildren
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Gets children of a content item with a given id
     *
     * ##usage
     * <pre>
     * contentResource.getChildren(1234, {pageSize: 10, pageNumber: 2})
     *    .then(function(contentArray) {
      *        var children = contentArray;
     *        alert('they are here!');
     *    });
      * </pre>
      *
     * @param {Int} parentid id of content item to return children of
     * @param {Object} options optional options object
     * @param {Int} options.pageSize if paging data, number of nodes per page, default = 0
     * @param {Int} options.pageNumber if paging data, current page index, default = 0
     * @param {String} options.filter if provided, query will only return those with names matching the filter
     * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
     * @param {String} options.orderBy property to order items by, default: `SortOrder`
      * @param {String} options.cultureName if provided, the results will be for this specific culture/variant
     * @returns {Promise} resourcePromise object containing an array of content items.
     *
     */
            getChildren: function getChildren(parentId, options) {
                var defaults = {
                    includeProperties: [],
                    pageSize: 0,
                    pageNumber: 0,
                    filter: '',
                    orderDirection: 'Ascending',
                    orderBy: 'SortOrder',
                    orderBySystemField: true,
                    cultureName: ''
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                //change asc/desct
                if (options.orderDirection === 'asc') {
                    options.orderDirection = 'Ascending';
                } else if (options.orderDirection === 'desc') {
                    options.orderDirection = 'Descending';
                }
                //converts the value to a js bool
                function toBool(v) {
                    if (Utilities.isNumber(v)) {
                        return v > 0;
                    }
                    if (Utilities.isString(v)) {
                        return v === 'true';
                    }
                    if (typeof v === 'boolean') {
                        return v;
                    }
                    return false;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetChildren', {
                    id: parentId,
                    includeProperties: _.pluck(options.includeProperties, 'alias').join(','),
                    pageNumber: options.pageNumber,
                    pageSize: options.pageSize,
                    orderBy: options.orderBy,
                    orderDirection: options.orderDirection,
                    orderBySystemField: toBool(options.orderBySystemField),
                    filter: options.filter,
                    cultureName: options.cultureName
                })), 'Failed to retrieve children for content item ' + parentId);
            },
            getDetailedPermissions: function getDetailedPermissions(contentId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetDetailedPermissions', { contentId: contentId })), 'Failed to retrieve permissions for content item ' + contentId);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#save
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Saves changes made to a content item to its current version, if the content item is new, the isNew parameter must be passed to force creation
      * if the content item needs to have files attached, they must be provided as the files param and passed separately
      *
      *
     * ##usage
     * <pre>
     * contentResource.getById(1234)
     *    .then(function(content) {
     *          content.name = "I want a new name!";
     *          contentResource.save(content, false)
     *            .then(function(content){
     *                alert("Retrieved, updated and saved again");
     *            });
     *    });
      * </pre>
      *
     * @param {Object} content The content item object with changes applied
      * @param {Bool} isNew set to true to create a new item or to update an existing
      * @param {Array} files collection of files for the document
      * @param {Bool} showNotifications an option to disable/show notifications (default is true)
     * @returns {Promise} resourcePromise object containing the saved content item.
     *
     */
            save: function save(content, isNew, files, showNotifications) {
                var endpoint = umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSave');
                return saveContentItem(content, 'save' + (isNew ? 'New' : ''), files, endpoint, showNotifications);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#saveBlueprint
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Saves changes made to a content blueprint item to its current version, if the content blueprint item is new, the isNew parameter must be passed to force creation
    * if the content item needs to have files attached, they must be provided as the files param and passed separately
    *
    * ##usage
    * <pre>
    * contentResource.getById(1234)
    *    .then(function(content) {
    *          content.name = "I want a new name!";
    *          contentResource.saveBlueprint(content, false)
    *            .then(function(content){
    *                alert("Retrieved, updated and saved again");
    *            });
    *    });
    * </pre>
    *
    * @param {Object} content The content blueprint item object with changes applied
    * @param {Bool} isNew set to true to create a new item or to update an existing
    * @param {Array} files collection of files for the document
    * @param {Bool} showNotifications an option to disable/show notifications (default is true)
    * @returns {Promise} resourcePromise object containing the saved content item.
    *
    */
            saveBlueprint: function saveBlueprint(content, isNew, files, showNotifications) {
                var endpoint = umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSaveBlueprint');
                return saveContentItem(content, 'save' + (isNew ? 'New' : ''), files, endpoint, showNotifications);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#publish
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Saves and publishes changes made to a content item to a new version, if the content item is new, the isNew parameter must be passed to force creation
      * if the content item needs to have files attached, they must be provided as the files param and passed separately
      *
      *
     * ##usage
     * <pre>
     * contentResource.getById(1234)
     *    .then(function(content) {
     *          content.name = "I want a new name, and be published!";
     *          contentResource.publish(content, false)
     *            .then(function(content){
     *                alert("Retrieved, updated and published again");
     *            });
     *    });
      * </pre>
      *
     * @param {Object} content The content item object with changes applied
      * @param {Bool} isNew set to true to create a new item or to update an existing
      * @param {Array} files collection of files for the document
      * @param {Bool} showNotifications an option to disable/show notifications (default is true)
     * @returns {Promise} resourcePromise object containing the saved content item.
     *
     */
            publish: function publish(content, isNew, files, showNotifications) {
                var endpoint = umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSave');
                return saveContentItem(content, 'publish' + (isNew ? 'New' : ''), files, endpoint, showNotifications);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#publish
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Saves and publishes changes made to a content item and its descendants to a new version, if the content item is new, the isNew parameter must be passed to force creation
    * if the content items needs to have files attached, they must be provided as the files param and passed separately
    *
    *
    * ##usage
    * <pre>
    * contentResource.getById(1234)
    *    .then(function(content) {
    *          content.name = "I want a new name, and be published!";
    *          contentResource.publishWithDescendants(content, false)
    *            .then(function(content){
    *                alert("Retrieved, updated and published again");
    *            });
    *    });
    * </pre>
    *
    * @param {Object} content The content item object with changes applied
    * @param {Bool} isNew set to true to create a new item or to update an existing
    * @param {Array} files collection of files for the document
    * @param {Bool} showNotifications an option to disable/show notifications (default is true)
    * @returns {Promise} resourcePromise object containing the saved content item.
    *
    */
            publishWithDescendants: function publishWithDescendants(content, isNew, force, files, showNotifications) {
                var endpoint = umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSave');
                var action = 'publishWithDescendants';
                if (force === true) {
                    action += 'Force';
                }
                return saveContentItem(content, action + (isNew ? 'New' : ''), files, endpoint, showNotifications);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#sendToPublish
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Saves changes made to a content item, and notifies any subscribers about a pending publication
      *
     * ##usage
     * <pre>
     * contentResource.getById(1234)
     *    .then(function(content) {
     *          content.name = "I want a new name, and be published!";
     *          contentResource.sendToPublish(content, false)
     *            .then(function(content){
     *                alert("Retrieved, updated and notication send off");
     *            });
     *    });
      * </pre>
      *
     * @param {Object} content The content item object with changes applied
      * @param {Bool} isNew set to true to create a new item or to update an existing
      * @param {Array} files collection of files for the document
     * @returns {Promise} resourcePromise object containing the saved content item.
     *
     */
            sendToPublish: function sendToPublish(content, isNew, files, showNotifications) {
                var endpoint = umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSave');
                return saveContentItem(content, 'sendPublish' + (isNew ? 'New' : ''), files, endpoint, showNotifications);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.contentResource#saveSchedule
      * @methodOf umbraco.resources.contentResource
      *
      * @description
      * Saves changes made to a content item, and saves the publishing schedule
      *
      * @param {Object} content The content item object with changes applied
      * @param {Bool} isNew set to true to create a new item or to update an existing
      * @param {Array} files collection of files for the document
      * @returns {Promise} resourcePromise object containing the saved content item.
      *
      */
            saveSchedule: function saveSchedule(content, isNew, files, showNotifications) {
                var endpoint = umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostSave');
                return saveContentItem(content, 'schedule' + (isNew ? 'New' : ''), files, endpoint, showNotifications);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#publishByid
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Publishes a content item with a given ID
      *
     * ##usage
     * <pre>
     * contentResource.publishById(1234)
     *    .then(function(content) {
     *        alert("published");
     *    });
      * </pre>
      *
     * @param {Int} id The ID of the conten to publish
     * @returns {Promise} resourcePromise object containing the published content item.
     *
     */
            publishById: function publishById(id) {
                if (!id) {
                    throw 'id cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostPublishById', [{ id: id }])), 'Failed to publish content with id ' + id);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentResource#createBlueprintFromContent
    * @methodOf umbraco.resources.contentResource
    *
    * @description
    * Creates a content blueprint with a given name from a given content id
    *
    * ##usage
    * <pre>
    * contentResource.createBlueprintFromContent(1234,"name")
    *    .then(function(content) {
    *        alert("created");
    *    });
        * </pre>
        *
    * @param {Int} id The ID of the content to create the content blueprint from
    * @param {string} id The name of the content blueprint
    * @returns {Promise} resourcePromise object
    *
    */
            createBlueprintFromContent: function createBlueprintFromContent(contentId, name) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'CreateBlueprintFromContent', {
                    contentId: contentId,
                    name: name
                })), 'Failed to create blueprint from content with id ' + contentId);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.contentResource#getRollbackVersions
      * @methodOf umbraco.resources.contentResource
      *
      * @description
      * Returns an array of previous version id's, given a node id and a culture
      *
      * ##usage
      * <pre>
      * contentResource.getRollbackVersions(id, culture)
      *    .then(function(versions) {
      *        alert('its here!');
      *    });
      * </pre>
      *
      * @param {Int} id Id of node
      * @param {Int} culture if provided, the results will be for this specific culture/variant
      * @returns {Promise} resourcePromise object containing the versions
      *
      */
            getRollbackVersions: function getRollbackVersions(contentId, culture) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetRollbackVersions', {
                    contentId: contentId,
                    culture: culture
                })), 'Failed to get rollback versions for content item with id ' + contentId);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.contentResource#getRollbackVersion
      * @methodOf umbraco.resources.contentResource
      *
      * @description
      * Returns a previous version of a content item
      *
      * ##usage
      * <pre>
      * contentResource.getRollbackVersion(versionId, culture)
      *    .then(function(version) {
      *        alert('its here!');
      *    });
      * </pre>
      *
      * @param {Int} versionId The version Id
      * @param {Int} culture if provided, the results will be for this specific culture/variant
      * @returns {Promise} resourcePromise object containing the version
      *
      */
            getRollbackVersion: function getRollbackVersion(versionId, culture) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetRollbackVersion', {
                    versionId: versionId,
                    culture: culture
                })), 'Failed to get version for content item with id ' + versionId);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.contentResource#rollback
      * @methodOf umbraco.resources.contentResource
      *
      * @description
      * Roll backs a content item to a previous version
      *
      * ##usage
      * <pre>
      * contentResource.rollback(contentId, versionId, culture)
      *    .then(function() {
      *        alert('its here!');
      *    });
      * </pre>
      *
      * @param {Int} id Id of node
      * @param {Int} versionId The version Id
      * @param {Int} culture if provided, the results will be for this specific culture/variant
      * @returns {Promise} resourcePromise object
      *
      */
            rollback: function rollback(contentId, versionId, culture) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostRollbackContent', {
                    contentId: contentId,
                    versionId: versionId,
                    culture: culture
                })), 'Failed to roll back content item with id ' + contentId);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.contentResource#getPublicAccess
      * @methodOf umbraco.resources.contentResource
      *
      * @description
      * Returns the public access protection for a content item
      *
      * ##usage
      * <pre>
      * contentResource.getPublicAccess(contentId)
      *    .then(function(publicAccess) {
      *        // do your thing
      *    });
      * </pre>
      *
      * @param {Int} contentId The content Id
      * @returns {Promise} resourcePromise object containing the public access protection
      *
      */
            getPublicAccess: function getPublicAccess(contentId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'GetPublicAccess', { contentId: contentId })), 'Failed to get public access for content item with id ' + contentId);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.contentResource#updatePublicAccess
      * @methodOf umbraco.resources.contentResource
      *
      * @description
      * Sets or updates the public access protection for a content item
      *
      * ##usage
      * <pre>
      * contentResource.updatePublicAccess(contentId, userName, password, roles, loginPageId, errorPageId)
      *    .then(function() {
      *        // do your thing
      *    });
      * </pre>
      *
      * @param {Int} contentId The content Id
      * @param {Array} groups The names of the groups that should have access (if using group based protection)
      * @param {Array} usernames The usernames of the members that should have access (if using member based protection)
      * @param {Int} loginPageId The Id of the login page
      * @param {Int} errorPageId The Id of the error page
      * @returns {Promise} resourcePromise object containing the public access protection
      *
      */
            updatePublicAccess: function updatePublicAccess(contentId, groups, usernames, loginPageId, errorPageId) {
                var publicAccess = {
                    contentId: contentId,
                    loginPageId: loginPageId,
                    errorPageId: errorPageId
                };
                if (Utilities.isArray(groups) && groups.length) {
                    publicAccess.groups = groups;
                } else if (Utilities.isArray(usernames) && usernames.length) {
                    publicAccess.usernames = usernames;
                } else {
                    throw 'must supply either userName/password or roles';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'PostPublicAccess', publicAccess)), 'Failed to update public access for content item with id ' + contentId);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.contentResource#removePublicAccess
      * @methodOf umbraco.resources.contentResource
      *
      * @description
      * Removes the public access protection for a content item
      *
      * ##usage
      * <pre>
      * contentResource.removePublicAccess(contentId)
      *    .then(function() {
      *        // do your thing
      *    });
      * </pre>
      *
      * @param {Int} contentId The content Id
      * @returns {Promise} resourcePromise object that's resolved once the public access has been removed
      *
      */
            removePublicAccess: function removePublicAccess(contentId) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentApiBaseUrl', 'RemovePublicAccess', { contentId: contentId })), 'Failed to remove public access for content item with id ' + contentId);
            }
        };
    }
    angular.module('umbraco.resources').factory('contentResource', contentResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.contentTypeResource
    * @description Loads in data for content types
    **/
    function contentTypeResource($q, $http, umbRequestHelper, umbDataFormatter, localizationService, notificationsService) {
        return {
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#getCount
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Gets the count of content types
    *
    * ##usage
    * <pre>
    * contentTypeResource.getCount()
    *    .then(function(data) {
    *        console.log(data);
    *    });
    * </pre>
    * 
    * @returns {Promise} resourcePromise object.
    *
    */
            getCount: function getCount() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetCount')), 'Failed to retrieve count');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#getAvailableCompositeContentTypes
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Gets the compositions for a content type
    *
    * ##usage
    * <pre>
    * contentTypeResource.getAvailableCompositeContentTypes()
    *    .then(function(data) {
    *        console.log(data);
    *    });
    * </pre>
    *
    * @param {Int} contentTypeId id of the content type to retrieve the list of the compositions
    * @param {Array} filterContentTypes array of content types to filter out 
    * @param {Array} filterPropertyTypes array of property aliases to filter out. If specified any content types with the property aliases will be filtered out
    * @param {Boolean} isElement whether the composite content types should be applicable for an element type
    * @returns {Promise} resourcePromise object.
    *
    */
            getAvailableCompositeContentTypes: function getAvailableCompositeContentTypes(contentTypeId, filterContentTypes, filterPropertyTypes, isElement) {
                if (!filterContentTypes) {
                    filterContentTypes = [];
                }
                if (!filterPropertyTypes) {
                    filterPropertyTypes = [];
                }
                var query = {
                    contentTypeId: contentTypeId,
                    filterContentTypes: filterContentTypes,
                    filterPropertyTypes: filterPropertyTypes,
                    isElement: isElement
                };
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetAvailableCompositeContentTypes'), query), 'Failed to retrieve data for content type id ' + contentTypeId);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#getWhereCompositionIsUsedInContentTypes
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Returns a list of content types which use a specific composition with a given id
    *
    * ##usage
    * <pre>
    * contentTypeResource.getWhereCompositionIsUsedInContentTypes(1234)
    *    .then(function(contentTypeList) {
    *        console.log(contentTypeList);
    *    });
    * </pre>
    * @param {Int} contentTypeId id of the composition content type to retrieve the list of the content types where it has been used
    * @returns {Promise} resourcePromise object.
    *
    */
            getWhereCompositionIsUsedInContentTypes: function getWhereCompositionIsUsedInContentTypes(contentTypeId) {
                var query = { contentTypeId: contentTypeId };
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetWhereCompositionIsUsedInContentTypes'), query), 'Failed to retrieve data for content type id ' + contentTypeId);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentTypeResource#getAllowedTypes
     * @methodOf umbraco.resources.contentTypeResource
     *
     * @description
     * Returns a list of allowed content types underneath a content item with a given ID
     *
     * ##usage
     * <pre>
     * contentTypeResource.getAllowedTypes(1234)
     *    .then(function(array) {
     *        $scope.type = type;
     *    });
     * </pre>
     * 
     * @param {Int} contentTypeId id of the content item to retrive allowed child types for
     * @returns {Promise} resourcePromise object.
     *
     */
            getAllowedTypes: function getAllowedTypes(contentTypeId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetAllowedChildren', [{ contentId: contentTypeId }])), 'Failed to retrieve data for content id ' + contentTypeId);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentTypeResource#getAllPropertyTypeAliases
     * @methodOf umbraco.resources.contentTypeResource
     *
     * @description
     * Returns a list of defined property type aliases
     *
     * ##usage
     * <pre>
     * contentTypeResource.getAllPropertyTypeAliases()
     *    .then(function(array) {
     *       Do stuff...
     *    });
     * </pre>
     *
     * @returns {Promise} resourcePromise object.
     *
     */
            getAllPropertyTypeAliases: function getAllPropertyTypeAliases() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetAllPropertyTypeAliases')), 'Failed to retrieve property type aliases');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#getAllStandardFields
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Returns a list of standard property type aliases
    *
    * ##usage
    * <pre>
    * contentTypeResource.getAllStandardFields()
    *    .then(function(array) {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @returns {Promise} resourcePromise object.
    *
    */
            getAllStandardFields: function getAllStandardFields() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetAllStandardFields')), 'Failed to retrieve standard fields');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#getPropertyTypeScaffold
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Returns the property display for a given datatype id
    *
    * ##usage
    * <pre>
    * contentTypeResource.getPropertyTypeScaffold(1234)
    *    .then(function(array) {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {Int} id the id of the datatype
    * @returns {Promise} resourcePromise object.
    *
    */
            getPropertyTypeScaffold: function getPropertyTypeScaffold(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetPropertyTypeScaffold', [{ id: id }])), 'Failed to retrieve property type scaffold');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#getById
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Get the content type with a given id
    *
    * ##usage
    * <pre>
    * contentTypeResource.getById("64058D0F-4911-4AB7-B3BA-000D89F00A26")
    *    .then(function(array) {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {String} id the guid id of the content type
    * @returns {Promise} resourcePromise object.
    *
    */
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to retrieve content type');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#deleteById
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Delete the content type of a given id
    *
    * ##usage
    * <pre>
    * contentTypeResource.deleteById(1234)
    *    .then(function(array) {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {Int} id the id of the content type
    * @returns {Promise} resourcePromise object.
    *
    */
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete content type');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#deleteContainerById
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Delete the content type container of a given id
    *
    * ##usage
    * <pre>
    * contentTypeResource.deleteContainerById(1234)
    *    .then(function(array) {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {Int} id the id of the content type container
    * @returns {Promise} resourcePromise object.
    *
    */
            deleteContainerById: function deleteContainerById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'DeleteContainer', [{ id: id }])), 'Failed to delete content type contaier');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#getAll
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Returns a list of all content types
    *
    * @returns {Promise} resourcePromise object.
    *
    */
            getAll: function getAll() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetAll')), 'Failed to retrieve all content types');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#getScaffold
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Returns an empty content type for use as a scaffold when creating a new content type
    *
    * ##usage
    * <pre>
    * contentTypeResource.getScaffold(1234)
    *    .then(function(array) {
    *       Do stuff...
    *    });
    * </pre>
    *
    * @param {Int} id the parent id
    * @returns {Promise} resourcePromise object.
    *
    */
            getScaffold: function getScaffold(parentId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'GetEmpty', { parentId: parentId })), 'Failed to retrieve content type scaffold');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentTypeResource#save
     * @methodOf umbraco.resources.contentTypeResource
     *
     * @description
     * Saves or update a content type
     *
     * @param {Object} content data type object to create/update
     * @returns {Promise} resourcePromise object.
     *
     */
            save: function save(contentType) {
                var saveModel = umbDataFormatter.formatContentTypePostData(contentType);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'PostSave'), saveModel), 'Failed to save data for content type id ' + contentType.id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentTypeResource#move
     * @methodOf umbraco.resources.contentTypeResource
     *
     * @description
     * Moves a node underneath a new parentId
     *
     * ##usage
     * <pre>
     * contentTypeResource.move({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("content type was moved");
     *    }, function(err){
     *      alert("content type didnt move:" + err.data.Message);
     *    });
     * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.id the ID of the content type to move
     * @param {Int} args.parentId the ID of the parent content type to move to
     * @returns {Promise} resourcePromise object.
     *
     */
            move: function move(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                var promise = localizationService.localize('contentType_moveFailed');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'PostMove'), {
                    parentId: args.parentId,
                    id: args.id
                }, { responseType: 'text' }), promise);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentTypeResource#copy
     * @methodOf umbraco.resources.contentTypeResource
     *
     * @description
     * Copied a content type underneath a new parentId
     *
     * ##usage
     * <pre>
     * contentTypeResource.copy({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("content type was copied");
     *    }, function(err){
     *      alert("content type didnt copy:" + err.data.Message);
     *    });
     * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.id the ID of the content type to copy
     * @param {Int} args.parentId the ID of the parent content type to copy to
     * @returns {Promise} resourcePromise object.
     *
     */
            copy: function copy(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                var promise = localizationService.localize('contentType_copyFailed');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'PostCopy'), {
                    parentId: args.parentId,
                    id: args.id
                }, { responseType: 'text' }), promise);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#createContainer
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Create a new content type container of a given name underneath a given parent item
    *
    * ##usage
    * <pre>
    * contentTypeResource.createContainer(1244,"testcontainer")
    *    .then(function() {
    *       Do stuff..
    *    });
    * </pre>
    * 
    * @param {Int} parentId the ID of the parent content type underneath which to create the container
    * @param {String} name the name of the container
    * @returns {Promise} resourcePromise object.
    *
    */
            createContainer: function createContainer(parentId, name) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'PostCreateContainer', {
                    parentId: parentId,
                    name: encodeURIComponent(name)
                })), 'Failed to create a folder under parent id ' + parentId);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#renameContainer
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Rename a container of a given id
    *
    * ##usage
    * <pre>
    * contentTypeResource.renameContainer( 1244,"testcontainer")
    *    .then(function() {
    *       Do stuff..
    *    });
    * </pre>
    *
    * @param {Int} id the ID of the container to rename
    * @param {String} name the new name of the container
    * @returns {Promise} resourcePromise object.
    *
    */
            renameContainer: function renameContainer(id, name) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'PostRenameContainer', {
                    id: id,
                    name: name
                })), 'Failed to rename the folder with id ' + id);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#export
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Export a content type of a given id.
    *
    * ##usage
    * <pre>
    * contentTypeResource.export(1234){
    *    .then(function() {
    *       Do stuff..
    *    });
    * </pre>
    *
    * @param {Int} id the ID of the container to rename
    * @param {String} name the new name of the container
    * @returns {Promise} resourcePromise object.
    *
    */
            export: function _export(id) {
                if (!id) {
                    throw 'id cannot be null';
                }
                var url = umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'Export', { id: id });
                return umbRequestHelper.downloadFile(url).then(function () {
                    localizationService.localize('speechBubbles_documentTypeExportedSuccess').then(function (value) {
                        notificationsService.success(value);
                    });
                }, function (data) {
                    localizationService.localize('speechBubbles_documentTypeExportedError').then(function (value) {
                        notificationsService.error(value);
                    });
                });
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#import
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Import a content type from a file
    *
    * ##usage
    * <pre>
    * contentTypeResource.import("path to file"){
    *    .then(function() {
    *       Do stuff..
    *    });
    * </pre>
    *
    * @param {String} file path of the file to import
    * @returns {Promise} resourcePromise object.
    *
    */
            import: function _import(file) {
                if (!file) {
                    throw 'file cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'Import', { file: file })), 'Failed to import document type ' + file);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#createDefaultTemplate
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Create a default template for a content type with a given id
    *
    * ##usage
    * <pre>
    * contentTypeResource.createDefaultTemplate(1234){
    *    .then(function() {
    *       Do stuff..
    *    });
    * </pre>
    *
    * @param {Int} id the id of the content type for which to create the default template
    * @returns {Promise} resourcePromise object.
    *
    */
            createDefaultTemplate: function createDefaultTemplate(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'PostCreateDefaultTemplate', { id: id })), 'Failed to create default template for content type with id ' + id);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.contentTypeResource#hasContentNodes
    * @methodOf umbraco.resources.contentTypeResource
    *
    * @description
    * Returns whether a content type has content nodes
    *
    * ##usage
    * <pre>
    * contentTypeResource.hasContentNodes(1234){
    *    .then(function() {
    *       Do stuff..
    *    });
    * </pre>
    *
    * @param {Int} id the id of the content type
    * @returns {Promise} resourcePromise object.
    *
    */
            hasContentNodes: function hasContentNodes(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('contentTypeApiBaseUrl', 'HasContentNodes', [{ id: id }])), 'Failed to retrieve indication for whether content type with id ' + id + ' has associated content nodes');
            }
        };
    }
    angular.module('umbraco.resources').factory('contentTypeResource', contentTypeResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.currentUserResource
    * @description Used for read/updates for the currently logged in user
    * 
    *
    **/
    function currentUserResource($q, $http, umbRequestHelper, umbDataFormatter) {
        //the factory object returned
        return {
            getPermissions: function getPermissions(nodeIds) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('currentUserApiBaseUrl', 'GetPermissions'), nodeIds), 'Failed to get permissions');
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.currentUserResource#hasPermission
      * @methodOf umbraco.resources.currentUserResource
      *
      * @description
      * Returns true/false given a permission char to check against a nodeID
      * for the current user
      *
      * ##usage
      * <pre>
      * contentResource.hasPermission('p',1234)
      *    .then(function() {
      *        alert('You are allowed to publish this item');
      *    });
      * </pre> 
      *
      * @param {String} permission char representing the permission to check
      * @param {Int} id id of content item to delete        
      * @returns {Promise} resourcePromise object.
      *
      */
            checkPermission: function checkPermission(permission, id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('currentUserApiBaseUrl', 'HasPermission', [
                    { permissionToCheck: permission },
                    { nodeId: id }
                ])), 'Failed to check permission for item ' + id);
            },
            saveTourStatus: function saveTourStatus(tourStatus) {
                if (!tourStatus) {
                    return $q.reject({ errorMsg: 'tourStatus cannot be empty' });
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('currentUserApiBaseUrl', 'PostSetUserTour'), tourStatus), 'Failed to save tour status');
            },
            getTours: function getTours() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('currentUserApiBaseUrl', 'GetUserTours')), 'Failed to get tours');
            },
            performSetInvitedUserPassword: function performSetInvitedUserPassword(newPassword) {
                if (!newPassword) {
                    return $q.reject({ errorMsg: 'newPassword cannot be empty' });
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('currentUserApiBaseUrl', 'PostSetInvitedUserPassword'), Utilities.toJson(newPassword)), 'Failed to change password');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.currentUserResource#changePassword
     * @methodOf umbraco.resources.currentUserResource
     *
     * @description
     * Changes the current users password
     * 
     * @returns {Promise} resourcePromise object containing the user array.
     *
     */
            changePassword: function changePassword(changePasswordArgs) {
                changePasswordArgs = umbDataFormatter.formatChangePasswordModel(changePasswordArgs);
                if (!changePasswordArgs) {
                    throw 'No password data to change';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('currentUserApiBaseUrl', 'PostChangePassword'), changePasswordArgs), 'Failed to change password');
            }
        };
    }
    angular.module('umbraco.resources').factory('currentUserResource', currentUserResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.dashboardResource
    * @description Handles loading the dashboard manifest
    **/
    function dashboardResource($q, $http, umbRequestHelper) {
        //the factory object returned
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.dashboardResource#getDashboard
     * @methodOf umbraco.resources.dashboardResource
     *
     * @description
     * Retrieves the dashboard configuration for a given section
     *
     * @param {string} section Alias of section to retrieve dashboard configuraton for
     * @returns {Promise} resourcePromise object containing the user array.
     *
     */
            getDashboard: function getDashboard(section) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dashboardApiBaseUrl', 'GetDashboard', [{ section: section }])), 'Failed to get dashboard ' + section);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.dashboardResource#getRemoteDashboardContent
    * @methodOf umbraco.resources.dashboardResource
    *
    * @description
    * Retrieves dashboard content from a remote source for a given section
    *
    * @param {string} section Alias of section to retrieve dashboard content for
    * @returns {Promise} resourcePromise object containing the user array.
    *
    */
            getRemoteDashboardContent: function getRemoteDashboardContent(section, baseurl) {
                //build request values with optional params
                var values = [{ section: section }];
                if (baseurl) {
                    values.push({ baseurl: baseurl });
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dashboardApiBaseUrl', 'GetRemoteDashboardContent', values)), 'Failed to get dashboard content');
            },
            getRemoteDashboardCssUrl: function getRemoteDashboardCssUrl(section, baseurl) {
                //build request values with optional params
                var values = [{ section: section }];
                if (baseurl) {
                    values.push({ baseurl: baseurl });
                }
                return umbRequestHelper.getApiUrl('dashboardApiBaseUrl', 'GetRemoteDashboardCss', values);
            },
            getRemoteXmlData: function getRemoteXmlData(site, url) {
                //build request values with optional params
                var values = {
                    site: site,
                    url: url
                };
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dashboardApiBaseUrl', 'GetRemoteXml', values)), 'Failed to get remote xml');
            }
        };
    }
    angular.module('umbraco.resources').factory('dashboardResource', dashboardResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.dataTypeResource
    * @description Loads in data for data types
    **/
    function dataTypeResource($q, $http, umbDataFormatter, umbRequestHelper) {
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getPreValues
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Retrieves available prevalues for a given data type + editor
     *
     * ##usage
     * <pre>
     * dataTypeResource.getPreValues("Umbraco.MediaPicker", 1234)
     *    .then(function(prevalues) {
     *        alert('its gone!');
     *    });
     * </pre>
     *
     * @param {String} editorAlias string alias of editor type to retrive prevalues configuration for
     * @param {Int} id id of datatype to retrieve prevalues for
     * @returns {Promise} resourcePromise object.
     *
     */
            getPreValues: function getPreValues(editorAlias, dataTypeId) {
                if (!dataTypeId) {
                    dataTypeId = -1;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetPreValues', [
                    { editorAlias: editorAlias },
                    { dataTypeId: dataTypeId }
                ])), 'Failed to retrieve pre values for editor alias ' + editorAlias);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getReferences
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Retrieves references of a given data type.
     *
     * @param {Int} id id of datatype to retrieve references for
     * @returns {Promise} resourcePromise object.
     *
     */
            getReferences: function getReferences(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetReferences', { id: id })), 'Failed to retrieve usages for data type of id ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getById
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Gets a data type item with a given id
     *
     * ##usage
     * <pre>
     * dataTypeResource.getById(1234)
     *    .then(function(datatype) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {Int} id id of data type to retrieve
     * @returns {Promise} resourcePromise object.
     *
     */
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to retrieve data for data type id ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getByName
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Gets a data type item with a given name
     *
     * ##usage
     * <pre>
     * dataTypeResource.getByName("upload")
     *    .then(function(datatype) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {String} name Name of data type to retrieve
     * @returns {Promise} resourcePromise object.
     *
     */
            getByName: function getByName(name) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetByName', [{ name: name }])), 'Failed to retrieve data for data type with name: ' + name);
            },
            getAll: function getAll() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetAll')), 'Failed to retrieve data');
            },
            getGroupedDataTypes: function getGroupedDataTypes() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetGroupedDataTypes')), 'Failed to retrieve data');
            },
            getGroupedPropertyEditors: function getGroupedPropertyEditors() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetGroupedPropertyEditors')), 'Failed to retrieve data');
            },
            getAllPropertyEditors: function getAllPropertyEditors() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetAllPropertyEditors')), 'Failed to retrieve data');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#getScaffold
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Returns a scaffold of an empty data type item
     *
     * The scaffold is used to build editors for data types that has not yet been populated with data.
     *
     * ##usage
     * <pre>
     * dataTypeResource.getScaffold()
     *    .then(function(scaffold) {
     *        var myType = scaffold;
     *        myType.name = "My new data type";
     *
     *        dataTypeResource.save(myType, myType.preValues, true)
     *            .then(function(type){
     *                alert("Retrieved, updated and saved again");
     *            });
     *    });
     * </pre>
     *
     * @returns {Promise} resourcePromise object containing the data type scaffold.
     *
     */
            getScaffold: function getScaffold(parentId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetEmpty', { parentId: parentId })), 'Failed to retrieve data for empty datatype');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#deleteById
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Deletes a data type with a given id
     *
     * ##usage
     * <pre>
     * dataTypeResource.deleteById(1234)
     *    .then(function() {
     *        alert('its gone!');
     *    });
     * </pre>
     *
     * @param {Int} id id of content item to delete
     * @returns {Promise} resourcePromise object.
     *
     */
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete item ' + id);
            },
            deleteContainerById: function deleteContainerById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'DeleteContainer', [{ id: id }])), 'Failed to delete content type contaier');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#getCustomListView
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Returns a custom listview, given a content types alias
     *
     *
     * ##usage
     * <pre>
     * dataTypeResource.getCustomListView("home")
     *    .then(function(listview) {
     *    });
     * </pre>
     *
     * @returns {Promise} resourcePromise object containing the listview datatype.
     *
     */
            getCustomListView: function getCustomListView(contentTypeAlias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'GetCustomListView', { contentTypeAlias: contentTypeAlias })), 'Failed to retrieve data for custom listview datatype');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.dataTypeResource#createCustomListView
    * @methodOf umbraco.resources.dataTypeResource
    *
    * @description
    * Creates and returns a custom listview, given a content types alias
    *
    * ##usage
    * <pre>
    * dataTypeResource.createCustomListView("home")
    *    .then(function(listview) {
    *    });
    * </pre>
    *
    * @returns {Promise} resourcePromise object containing the listview datatype.
    *
    */
            createCustomListView: function createCustomListView(contentTypeAlias) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'PostCreateCustomListView', { contentTypeAlias: contentTypeAlias })), 'Failed to create a custom listview datatype');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#save
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Saves or update a data type
     *
     * @param {Object} dataType data type object to create/update
     * @param {Array} preValues collection of prevalues on the datatype
     * @param {Bool} isNew set to true if type should be create instead of updated
     * @returns {Promise} resourcePromise object.
     *
     */
            save: function save(dataType, preValues, isNew) {
                var saveModel = umbDataFormatter.formatDataTypePostData(dataType, preValues, 'save' + (isNew ? 'New' : ''));
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'PostSave'), saveModel), 'Failed to save data for data type id ' + dataType.id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.dataTypeResource#move
     * @methodOf umbraco.resources.dataTypeResource
     *
     * @description
     * Moves a node underneath a new parentId
     *
     * ##usage
     * <pre>
     * dataTypeResource.move({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("node was moved");
     *    }, function(err){
     *      alert("node didnt move:" + err.data.Message); 
     *    });
     * </pre> 
     * @param {Object} args arguments object
     * @param {Int} args.idd the ID of the node to move
     * @param {Int} args.parentId the ID of the parent node to move to
     * @returns {Promise} resourcePromise object.
     *
     */
            move: function move(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'PostMove'), {
                    parentId: args.parentId,
                    id: args.id
                }, { responseType: 'text' }), 'Failed to move content');
            },
            createContainer: function createContainer(parentId, name) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'PostCreateContainer', {
                    parentId: parentId,
                    name: encodeURIComponent(name)
                })), 'Failed to create a folder under parent id ' + parentId);
            },
            renameContainer: function renameContainer(id, name) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dataTypeApiBaseUrl', 'PostRenameContainer', {
                    id: id,
                    name: encodeURIComponent(name)
                })), 'Failed to rename the folder with id ' + id);
            }
        };
    }
    angular.module('umbraco.resources').factory('dataTypeResource', dataTypeResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.dictionaryResource
    * @description Loads in data for dictionary items
**/
    function dictionaryResource($q, $http, $location, umbRequestHelper, umbDataFormatter) {
        /**
         * @ngdoc method
         * @name umbraco.resources.dictionaryResource#deleteById
         * @methodOf umbraco.resources.dictionaryResource
         *
         * @description
         * Deletes a dictionary item with a given id
         *
         * ##usage
         * <pre>
         * dictionaryResource.deleteById(1234)
         *    .then(function() {
         *        alert('its gone!');
         *    });
         * </pre>
         *
         * @param {Int} id id of dictionary item to delete
         * @returns {Promise} resourcePromise object.
         *
  **/
        function deleteById(id) {
            return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dictionaryApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete item ' + id);
        }
        /**
         * @ngdoc method
         * @name umbraco.resources.dictionaryResource#create
         * @methodOf umbraco.resources.dictionaryResource
         *
         * @description
         * Creates a dictionary item with the gieven key and parent id
         *
         * ##usage
         * <pre>
         * dictionaryResource.create(1234,"Item key")
         *    .then(function() {
         *        alert('its created!');
         *    });
         * </pre>
         *
         * @param {Int} parentid the parentid of the new dictionary item
         * @param {String} key the key of the new dictionary item
         * @returns {Promise} resourcePromise object.
         *
  **/
        function create(parentid, key) {
            return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dictionaryApiBaseUrl', 'Create', {
                parentId: parentid,
                key: key
            })), 'Failed to create item ');
        }
        /**
       * @ngdoc method
       * @name umbraco.resources.dictionaryResource#deleteById
       * @methodOf umbraco.resources.dictionaryResource
       *
       * @description
       * Gets a dictionary item with a given id
       *
       * ##usage
       * <pre>
       * dictionaryResource.getById(1234)
       *    .then(function() {
       *        alert('Found it!');
       *    });
       * </pre>
       *
       * @param {Int} id id of dictionary item to get
       * @returns {Promise} resourcePromise object.
       *
  **/
        function getById(id) {
            return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dictionaryApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to get item ' + id);
        }
        /**
      * @ngdoc method
      * @name umbraco.resources.dictionaryResource#save
      * @methodOf umbraco.resources.dictionaryResource
      *
      * @description
      * Updates a dictionary
      *
      * @param {Object} dictionary  dictionary object to update     
      * @param {Bool} nameIsDirty set to true if the name has been changed
      * @returns {Promise} resourcePromise object.
      *
      */
        function save(dictionary, nameIsDirty) {
            var saveModel = umbDataFormatter.formatDictionaryPostData(dictionary, nameIsDirty);
            return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('dictionaryApiBaseUrl', 'PostSave'), saveModel), 'Failed to save data for dictionary id ' + dictionary.id);
        }
        /**
       * @ngdoc method
       * @name umbraco.resources.dictionaryResource#getList
       * @methodOf umbraco.resources.dictionaryResource
       *
       * @description
       * Gets a list of all dictionary items
       *
       * ##usage
       * <pre>
       * dictionaryResource.getList()
       *    .then(function() {
       *        alert('Found it!');
       *    });
       * </pre>
       *         
       * @returns {Promise} resourcePromise object.
       *
  **/
        function getList() {
            return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('dictionaryApiBaseUrl', 'getList')), 'Failed to get list');
        }
        var resource = {
            deleteById: deleteById,
            create: create,
            getById: getById,
            save: save,
            getList: getList
        };
        return resource;
    }
    angular.module('umbraco.resources').factory('dictionaryResource', dictionaryResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.elementTypeResource
    * @description Loads in data for element types
    **/
    function elementTypeResource($q, $http, umbRequestHelper) {
        return {
            /**
    * @ngdoc method
    * @name umbraco.resources.elementTypeResource#getAll
    * @methodOf umbraco.resources.elementTypeResource
    *
    * @description
    * Gets a list of all element types
    *
    * ##usage
    * <pre>
    * elementTypeResource.getAll()
    *    .then(function() {
    *        alert('Found it!');
    *    });
    * </pre>
    *
    * @returns {Promise} resourcePromise object.
    *
    **/
            getAll: function getAll() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('elementTypeApiBaseUrl', 'GetAll')), 'Failed to retrieve element types');
            }
        };
    }
    angular.module('umbraco.resources').factory('elementTypeResource', elementTypeResource);
    'use strict';
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
        var emailApiUrl = 'https://emailcollector.umbraco.io/api/EmailProxy';
        //the factory object returned
        return {
            postAddUserToEmailMarketing: function postAddUserToEmailMarketing(user) {
                return umbRequestHelper.resourcePromise($http.post(emailApiUrl, {
                    name: user.name,
                    email: user.email,
                    usergroup: user.userGroups
                }), 'Failed to add user to email marketing list');
            }
        };
    }
    angular.module('umbraco.resources').factory('emailMarketingResource', emailMarketingResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.entityResource
    * @description Loads in basic data for all entities
    *
    * ##What is an entity?
    * An entity is a basic **read-only** representation of an Umbraco node. It contains only the most
    * basic properties used to display the item in trees, lists and navigation.
    *
    * ##What is the difference between entity and content/media/etc...?
    * the entity only contains the basic node data, name, id and guid, whereas content
    * nodes fetched through the content service also contains additional all of the content property data, etc..
    * This is the same principal for all entity types. Any user that is logged in to the back office will have access
    * to view the basic entity information for all entities since the basic entity information does not contain sensitive information.
    *
    * ##Entity object types?
    * You need to specify the type of object you want returned.
    *
    * The core object types are:
    *
    * - Document
    * - Media
    * - Member
    * - Template
    * - DocumentType
    * - MediaType
    * - MemberType
    * - Macro
    * - User
    * - Language
    * - Domain
    * - DataType
    **/
    function entityResource($q, $http, umbRequestHelper) {
        //the factory object returned
        return {
            getSafeAlias: function getSafeAlias(value, camelCase) {
                if (!value) {
                    return '';
                }
                value = value.replace('#', '');
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetSafeAlias', {
                    value: encodeURIComponent(value),
                    camelCase: camelCase
                })), 'Failed to retrieve content type scaffold');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#getPath
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Returns a path, given a node ID and type
     *
     * ##usage
     * <pre>
     * entityResource.getPath(id, type)
     *    .then(function(pathArray) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {Int} id Id of node to return the public url to
     * @param {string} type Object type name
     * @returns {Promise} resourcePromise object containing the url.
     *
     */
            getPath: function getPath(id, type) {
                if (id === -1 || id === '-1') {
                    return '-1';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetPath', [
                    { id: id },
                    { type: type }
                ])), 'Failed to retrieve path for id:' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#getUrl
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Returns a url, given a node ID and type
     *
     * ##usage
     * <pre>
     * entityResource.getUrl(id, type)
     *    .then(function(url) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {Int} id Id of node to return the public url to
     * @param {string} type Object type name
     * @param {string} culture Culture
     * @returns {Promise} resourcePromise object containing the url.
     *
     */
            getUrl: function getUrl(id, type, culture) {
                if (id === -1 || id === '-1') {
                    return '';
                }
                if (!culture) {
                    culture = '';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetUrl', [
                    { id: id },
                    { type: type },
                    { culture: culture }
                ])), 'Failed to retrieve url for id:' + id);
            },
            getUrlByUdi: function getUrlByUdi(udi, culture) {
                if (!udi) {
                    return '';
                }
                if (!culture) {
                    culture = '';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetUrl', [
                    { udi: udi },
                    { culture: culture }
                ])), 'Failed to retrieve url for UDI:' + udi);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#getById
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Gets an entity with a given id
     *
     * ##usage
     * <pre>
     * //get media by id
     * entityResource.getById(0, "Media")
     *    .then(function(ent) {
     *        var myDoc = ent;
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {Int} id id of entity to return
     * @param {string} type Object type name
     * @returns {Promise} resourcePromise object containing the entity.
     *
     */
            getById: function getById(id, type) {
                if (id === -1 || id === '-1') {
                    return null;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetById', [
                    { id: id },
                    { type: type }
                ])), 'Failed to retrieve entity data for id ' + id);
            },
            getUrlAndAnchors: function getUrlAndAnchors(id, culture) {
                if (id === -1 || id === '-1') {
                    return null;
                }
                if (!culture) {
                    culture = '';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetUrlAndAnchors', [
                    { id: id },
                    { culture: culture }
                ])), 'Failed to retrieve url and anchors data for id ' + id);
            },
            getAnchors: function getAnchors(rteContent) {
                if (!rteContent || rteContent.length === 0) {
                    return $q.when([]);
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetAnchors'), { rteContent: rteContent }), 'Failed to anchors data for rte content ' + rteContent);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#getByIds
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Gets an array of entities, given a collection of ids
     *
     * ##usage
     * <pre>
     * //Get templates for ids
     * entityResource.getByIds( [1234,2526,28262], "Template")
     *    .then(function(templateArray) {
     *        var myDoc = contentArray;
     *        alert('they are here!');
     *    });
     * </pre>
     *
     * @param {Array} ids ids of entities to return as an array
     * @param {string} type type name
     * @returns {Promise} resourcePromise object containing the entity array.
     *
     */
            getByIds: function getByIds(ids, type) {
                var query = 'type=' + type;
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetByIds', query), { ids: ids }), 'Failed to retrieve entity data for ids ' + ids);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#getByQuery
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Gets an entity from a given xpath
     *
     * ##usage
     * <pre>
     * //get content by xpath
     * entityResource.getByQuery("$current", -1, "Document")
     *    .then(function(ent) {
     *        var myDoc = ent;
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {string} query xpath to use in query
     * @param {Int} nodeContextId id id to start from
     * @param {string} type Object type name
     * @returns {Promise} resourcePromise object containing the entity.
     *
     */
            getByQuery: function getByQuery(query, nodeContextId, type) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetByQuery', [
                    { query: query },
                    { nodeContextId: nodeContextId },
                    { type: type }
                ])), 'Failed to retrieve entity data for query ' + query);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#getAll
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Gets an entity with a given id
     *
     * ##usage
     * <pre>
     *
     * //Only return media
     * entityResource.getAll("Media")
     *    .then(function(ent) {
     *        var myDoc = ent;
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {string} type Object type name
     * @param {string} postFilter optional filter expression which will execute a dynamic where clause on the server
     * @returns {Promise} resourcePromise object containing the entity.
     *
     */
            getAll: function getAll(type, postFilter) {
                //need to build the query string manually
                var query = 'type=' + type + '&postFilter=' + (postFilter ? encodeURIComponent(postFilter) : '');
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetAll', query)), 'Failed to retrieve entity data for type ' + type);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#getAncestors
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Gets ancestor entities for a given item
     *
     *
     * @param {string} type Object type name
     * @param {string} culture Culture
     * @returns {Promise} resourcePromise object containing the entity.
     *
     */
            getAncestors: function getAncestors(id, type, culture, options) {
                if (!culture) {
                    culture = '';
                }
                var args = [
                    { id: id },
                    { type: type },
                    { culture: culture }
                ];
                if (options && options.dataTypeKey) {
                    args.push({ dataTypeKey: options.dataTypeKey });
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetAncestors', args)), 'Failed to retrieve ancestor data for id ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#getChildren
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Gets children entities for a given item
     *
     * @param {Int} parentid id of content item to return children of
     * @param {string} type Object type name
     * @returns {Promise} resourcePromise object containing the entity.
     *
     */
            getChildren: function getChildren(id, type, options) {
                var args = [
                    { id: id },
                    { type: type }
                ];
                if (options && options.dataTypeKey) {
                    args.push({ dataTypeKey: options.dataTypeKey });
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetChildren', args)), 'Failed to retrieve child data for id ' + id);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.entityResource#getPagedChildren
      * @methodOf umbraco.resources.entityResource
      *
      * @description
      * Gets paged children of a content item with a given id
      *
      * ##usage
      * <pre>
      * entityResource.getPagedChildren(1234, "Content", {pageSize: 10, pageNumber: 2})
      *    .then(function(contentArray) {
      *        var children = contentArray;
      *        alert('they are here!');
      *    });
      * </pre>
      *
      * @param {Int} parentid id of content item to return children of
      * @param {string} type Object type name
      * @param {Object} options optional options object
      * @param {Int} options.pageSize if paging data, number of nodes per page, default = 1
      * @param {Int} options.pageNumber if paging data, current page index, default = 100
      * @param {String} options.filter if provided, query will only return those with names matching the filter
      * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
      * @param {String} options.orderBy property to order items by, default: `SortOrder`
      * @returns {Promise} resourcePromise object containing an array of content items.
      *
      */
            getPagedChildren: function getPagedChildren(parentId, type, options) {
                var defaults = {
                    pageSize: 1,
                    pageNumber: 100,
                    filter: '',
                    orderDirection: 'Ascending',
                    orderBy: 'SortOrder',
                    dataTypeKey: null
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                //change asc/desct
                if (options.orderDirection === 'asc') {
                    options.orderDirection = 'Ascending';
                } else if (options.orderDirection === 'desc') {
                    options.orderDirection = 'Descending';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetPagedChildren', {
                    id: parentId,
                    type: type,
                    pageNumber: options.pageNumber,
                    pageSize: options.pageSize,
                    orderBy: options.orderBy,
                    orderDirection: options.orderDirection,
                    filter: encodeURIComponent(options.filter),
                    dataTypeKey: options.dataTypeKey
                })), 'Failed to retrieve child data for id ' + parentId);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.entityResource#getPagedDescendants
      * @methodOf umbraco.resources.entityResource
      *
      * @description
      * Gets paged descendants of a content item with a given id
      *
      * ##usage
      * <pre>
      * entityResource.getPagedDescendants(1234, "Document", {pageSize: 10, pageNumber: 2})
      *    .then(function(contentArray) {
      *        var children = contentArray;
      *        alert('they are here!');
      *    });
      * </pre>
      *
      * @param {Int} parentid id of content item to return descendants of
      * @param {string} type Object type name
      * @param {Object} options optional options object
      * @param {Int} options.pageSize if paging data, number of nodes per page, default = 100
      * @param {Int} options.pageNumber if paging data, current page index, default = 1
      * @param {String} options.filter if provided, query will only return those with names matching the filter
      * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
      * @param {String} options.orderBy property to order items by, default: `SortOrder`
      * @returns {Promise} resourcePromise object containing an array of content items.
      *
      */
            getPagedDescendants: function getPagedDescendants(parentId, type, options) {
                var defaults = {
                    pageSize: 100,
                    pageNumber: 1,
                    filter: '',
                    orderDirection: 'Ascending',
                    orderBy: 'SortOrder',
                    dataTypeKey: null
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                //change asc/desct
                if (options.orderDirection === 'asc') {
                    options.orderDirection = 'Ascending';
                } else if (options.orderDirection === 'desc') {
                    options.orderDirection = 'Descending';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'GetPagedDescendants', {
                    id: parentId,
                    type: type,
                    pageNumber: options.pageNumber,
                    pageSize: options.pageSize,
                    orderBy: options.orderBy,
                    orderDirection: options.orderDirection,
                    filter: encodeURIComponent(options.filter),
                    dataTypeKey: options.dataTypeKey
                })), 'Failed to retrieve child data for id ' + parentId);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#search
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Gets an array of entities, given a lucene query and a type
     *
     * ##usage
     * <pre>
     * entityResource.search("news", "Media")
     *    .then(function(mediaArray) {
     *        var myDoc = mediaArray;
     *        alert('they are here!');
     *    });
     * </pre>
     *
     * @param {String} Query search query
     * @param {String} Type type of conten to search
     * @returns {Promise} resourcePromise object containing the entity array.
     *
     */
            search: function search(query, type, searchFrom, canceler, dataTypeKey) {
                var args = [
                    { query: query },
                    { type: type }
                ];
                if (searchFrom) {
                    args.push({ searchFrom: searchFrom });
                }
                if (dataTypeKey) {
                    args.push({ dataTypeKey: dataTypeKey });
                }
                var httpConfig = {};
                if (canceler) {
                    httpConfig['timeout'] = canceler;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'Search', args), httpConfig), 'Failed to retrieve entity data for query ' + query);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.entityResource#searchAll
     * @methodOf umbraco.resources.entityResource
     *
     * @description
     * Gets an array of entities from all available search indexes, given a lucene query
     *
     * ##usage
     * <pre>
     * entityResource.searchAll("bob")
     *    .then(function(array) {
     *        var myDoc = array;
     *        alert('they are here!');
     *    });
     * </pre>
     *
     * @param {String} Query search query
     * @returns {Promise} resourcePromise object containing the entity array.
     *
     */
            searchAll: function searchAll(query, canceler) {
                var httpConfig = {};
                if (canceler) {
                    httpConfig['timeout'] = canceler;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('entityApiBaseUrl', 'SearchAll', [{ query: query }]), httpConfig), 'Failed to retrieve entity data for query ' + query);
            }
        };
    }
    angular.module('umbraco.resources').factory('entityResource', entityResource);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.healthCheckResource
 * @function
 *
 * @description
 * Used by the health check dashboard to get checks and send requests to fix checks.
 */
    (function () {
        'use strict';
        function healthCheckResource($http, umbRequestHelper) {
            /**
     * @ngdoc function
     * @name umbraco.resources.healthCheckService#getAllChecks
     * @methodOf umbraco.resources.healthCheckResource
     * @function
     *
     * @description
     * Called to get all available health checks
     */
            function getAllChecks() {
                return umbRequestHelper.resourcePromise($http.get(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + 'GetAllHealthChecks'), 'Failed to retrieve health checks');
            }
            /**
     * @ngdoc function
     * @name umbraco.resources.healthCheckService#getStatus
     * @methodOf umbraco.resources.healthCheckResource
     * @function
     *
     * @description
     * Called to get execute a health check and return the check status
     */
            function getStatus(id) {
                return umbRequestHelper.resourcePromise($http.get(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + 'GetStatus?id=' + id), 'Failed to retrieve status for health check with ID ' + id);
            }
            /**
     * @ngdoc function
     * @name umbraco.resources.healthCheckService#executeAction
     * @methodOf umbraco.resources.healthCheckResource
     * @function
     *
     * @description
     * Called to execute a health check action (rectifying an issue)
     */
            function executeAction(action) {
                return umbRequestHelper.resourcePromise($http.post(Umbraco.Sys.ServerVariables.umbracoUrls.healthCheckBaseUrl + 'ExecuteAction', action), 'Failed to execute action with alias ' + action.alias + ' and healthCheckId + ' + action.healthCheckId);
            }
            var resource = {
                getAllChecks: getAllChecks,
                getStatus: getStatus,
                executeAction: executeAction
            };
            return resource;
        }
        angular.module('umbraco.resources').factory('healthCheckResource', healthCheckResource);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.imageUrlGeneratorResource
 * @function
 *
 * @description
 * Used by the various controllers to get an image URL formatted correctly for the current image URL generator
 */
    (function () {
        'use strict';
        function imageUrlGeneratorResource($http, umbRequestHelper) {
            function getCropUrl(mediaPath, width, height, imageCropMode, animationProcessMode) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('imageUrlGeneratorApiBaseUrl', 'GetCropUrl', {
                    mediaPath: mediaPath,
                    width: width,
                    height: height,
                    imageCropMode: imageCropMode,
                    animationProcessMode: animationProcessMode
                })), 'Failed to get crop URL');
            }
            var resource = { getCropUrl: getCropUrl };
            return resource;
        }
        angular.module('umbraco.resources').factory('imageUrlGeneratorResource', imageUrlGeneratorResource);
    }());
    'use strict';
    (function () {
        'use strict';
        /**
    * @ngdoc service
    * @name umbraco.resources.javascriptLibraryResource
    * @description Handles retrieving data for javascript libraries on the server
    **/
        function javascriptLibraryResource($q, $http, umbRequestHelper) {
            var existingLocales = null;
            function getSupportedLocales() {
                var deferred = $q.defer();
                if (existingLocales === null) {
                    umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('backOfficeAssetsApiBaseUrl', 'GetSupportedLocales')), 'Failed to get cultures').then(function (locales) {
                        existingLocales = locales;
                        deferred.resolve(existingLocales);
                    });
                } else {
                    deferred.resolve(existingLocales);
                }
                return deferred.promise;
            }
            var service = { getSupportedLocales: getSupportedLocales };
            return service;
        }
        angular.module('umbraco.resources').factory('javascriptLibraryResource', javascriptLibraryResource);
    }());
    'use strict';
    /**
  * @ngdoc service
  * @name umbraco.resources.languageResource
  * @description Handles retrieving and updating language data
  **/
    function languageResource($http, umbRequestHelper) {
        return {
            getCultures: function getCultures() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('languageApiBaseUrl', 'GetAllCultures')), 'Failed to get cultures');
            },
            getAll: function getAll() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('languageApiBaseUrl', 'GetAllLanguages')), 'Failed to get languages');
            },
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('languageApiBaseUrl', 'GetLanguage', { id: id })), 'Failed to get language with id ' + id);
            },
            save: function save(lang) {
                if (!lang)
                    throw '\'lang\' parameter cannot be null';
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('languageApiBaseUrl', 'SaveLanguage'), lang), 'Failed to save language ' + lang.id);
            },
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('languageApiBaseUrl', 'DeleteLanguage', { id: id })), 'Failed to delete item ' + id);
            }
        };
    }
    angular.module('umbraco.resources').factory('languageResource', languageResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.logResource
    * @description Retrives log history from umbraco
    * 
    *
    **/
    function logResource($q, $http, umbRequestHelper) {
        function isValidDate(input) {
            if (input) {
                if (Object.prototype.toString.call(input) === '[object Date]' && !isNaN(input.getTime())) {
                    return true;
                }
            }
            return false;
        }
        ;
        function dateToValidIsoString(input) {
            if (isValidDate(input)) {
                return input.toISOString();
            }
            return '';
        }
        ;
        //the factory object returned
        return {
            /**
    * @ngdoc method
    * @name umbraco.resources.logResource#getPagedEntityLog
    * @methodOf umbraco.resources.logResource
    *
    * @description
    * Gets a paginated log history for a entity
    *
    * ##usage
    * <pre>
    * var options = {
    *      id : 1234
    *      pageSize : 10,
    *      pageNumber : 1,
    *      orderDirection : "Descending",
    *      sinceDate : new Date(2018,0,1)
    * };
    * logResource.getPagedEntityLog(options)
    *    .then(function(log) {
    *        alert('its here!');
    *    });
    * </pre> 
    * 
    * @param {Object} options options object
    * @param {Int} options.id the id of the entity
    * @param {Int} options.pageSize if paging data, number of nodes per page, default = 10
    * @param {Int} options.pageNumber if paging data, current page index, default = 1
    * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Descending`
    * @param {Date} options.sinceDate if provided this will only get log entries going back to this date
    * @returns {Promise} resourcePromise object containing the log.
    *
    */
            getPagedEntityLog: function getPagedEntityLog(options) {
                var defaults = {
                    pageSize: 10,
                    pageNumber: 1,
                    orderDirection: 'Descending'
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                if (options.hasOwnProperty('sinceDate')) {
                    options.sinceDate = dateToValidIsoString(options.sinceDate);
                }
                //change asc/desct
                if (options.orderDirection === 'asc') {
                    options.orderDirection = 'Ascending';
                } else if (options.orderDirection === 'desc') {
                    options.orderDirection = 'Descending';
                }
                if (options.id === undefined || options.id === null) {
                    throw 'options.id is required';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('logApiBaseUrl', 'GetPagedEntityLog', options)), 'Failed to retrieve log data for id');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.logResource#getPagedUserLog
     * @methodOf umbraco.resources.logResource
     *
     * @description
     * Gets a paginated log history for the current user
     *
     * ##usage
     * <pre>
     * var options = {
     *      pageSize : 10,
     *      pageNumber : 1,
     *      orderDirection : "Descending",
     *      sinceDate : new Date(2018,0,1)
     * };
     * logResource.getPagedUserLog(options)
     *    .then(function(log) {
     *        alert('its here!');
     *    });
     * </pre> 
     * 
     * @param {Object} options options object
     * @param {Int} options.pageSize if paging data, number of nodes per page, default = 10
     * @param {Int} options.pageNumber if paging data, current page index, default = 1
     * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Descending`
     * @param {Date} options.sinceDate if provided this will only get log entries going back to this date
     * @returns {Promise} resourcePromise object containing the log.
     *
     */
            getPagedUserLog: function getPagedUserLog(options) {
                var defaults = {
                    pageSize: 10,
                    pageNumber: 1,
                    orderDirection: 'Descending'
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                if (options.hasOwnProperty('sinceDate')) {
                    options.sinceDate = dateToValidIsoString(options.sinceDate);
                }
                //change asc/desct
                if (options.orderDirection === 'asc') {
                    options.orderDirection = 'Ascending';
                } else if (options.orderDirection === 'desc') {
                    options.orderDirection = 'Descending';
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('logApiBaseUrl', 'GetPagedCurrentUserLog', options)), 'Failed to retrieve log data for id');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.logResource#getEntityLog
     * @methodOf umbraco.resources.logResource
     *
     * @description
     *  <strong>[OBSOLETE] use getPagedEntityLog instead</strong><br />
     * Gets the log history for a give entity id
     *
     * ##usage
     * <pre>
     * logResource.getEntityLog(1234)
     *    .then(function(log) {
     *        alert('its here!');
     *    });
     * </pre> 
     * 
     * @param {Int} id id of entity to return log history        
     * @returns {Promise} resourcePromise object containing the log.
     *
     */
            getEntityLog: function getEntityLog(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('logApiBaseUrl', 'GetEntityLog', [{ id: id }])), 'Failed to retrieve user data for id ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.logResource#getUserLog
     * @methodOf umbraco.resources.logResource
     *
     * @description
     * <strong>[OBSOLETE] use getPagedUserLog instead</strong><br />
     * Gets the current user's log history for a given type of log entry
     *
     * ##usage
     * <pre>
     * logResource.getUserLog("save", new Date())
     *    .then(function(log) {
     *        alert('its here!');
     *    });
     * </pre> 
     * 
     * @param {String} type logtype to query for
     * @param {DateTime} since query the log back to this date, by defalt 7 days ago
     * @returns {Promise} resourcePromise object containing the log.
     *
     */
            getUserLog: function getUserLog(type, since) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('logApiBaseUrl', 'GetCurrentUserLog', [
                    { logtype: type },
                    { sinceDate: dateToValidIsoString(since) }
                ])), 'Failed to retrieve log data for current user of type ' + type + ' since ' + since);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.logResource#getLog
     * @methodOf umbraco.resources.logResource
     *
     * @description
     * Gets the log history for a given type of log entry
     *
     * ##usage
     * <pre>
     * logResource.getLog("save", new Date())
     *    .then(function(log) {
     *        alert('its here!');
     *    });
     * </pre> 
     * 
     * @param {String} type logtype to query for
     * @param {DateTime} since query the log back to this date, by defalt 7 days ago
     * @returns {Promise} resourcePromise object containing the log.
     *
     */
            getLog: function getLog(type, since) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('logApiBaseUrl', 'GetLog', [
                    { logtype: type },
                    { sinceDate: dateToValidIsoString(since) }
                ])), 'Failed to retrieve log data of type ' + type + ' since ' + since);
            }
        };
    }
    angular.module('umbraco.resources').factory('logResource', logResource);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.logViewerResource
 * @description Retrives Umbraco log items (by default from JSON files on disk)
 *
 *
 **/
    function logViewerResource($q, $http, umbRequestHelper) {
        /**
   * verb => 'get', 'post',
   * method => API method to call
   * params => additional data to send
   * error => error message when things go wrong...
   */
        var request = function request(verb, method, params, error) {
            return umbRequestHelper.resourcePromise(verb === 'GET' ? $http.get(umbRequestHelper.getApiUrl('logViewerApiBaseUrl', method) + (params ? params : '')) : $http.post(umbRequestHelper.getApiUrl('logViewerApiBaseUrl', method), params), error);
        };
        //the factory object returned
        return {
            getNumberOfErrors: function getNumberOfErrors(startDate, endDate) {
                return request('GET', 'GetNumberOfErrors', '?startDate=' + startDate + '&endDate=' + endDate, 'Failed to retrieve number of errors in logs');
            },
            getLogLevel: function getLogLevel() {
                return request('GET', 'GetLogLevel', null, 'Failed to retrieve log level');
            },
            getLogLevelCounts: function getLogLevelCounts(startDate, endDate) {
                return request('GET', 'GetLogLevelCounts', '?startDate=' + startDate + '&endDate=' + endDate, 'Failed to retrieve log level counts');
            },
            getMessageTemplates: function getMessageTemplates(startDate, endDate) {
                return request('GET', 'GetMessageTemplates', '?startDate=' + startDate + '&endDate=' + endDate, 'Failed to retrieve log templates');
            },
            getSavedSearches: function getSavedSearches() {
                return request('GET', 'GetSavedSearches', null, 'Failed to retrieve saved searches');
            },
            postSavedSearch: function postSavedSearch(name, query) {
                return request('POST', 'PostSavedSearch', {
                    'name': name,
                    'query': query
                }, 'Failed to add new saved search');
            },
            deleteSavedSearch: function deleteSavedSearch(name, query) {
                return request('POST', 'DeleteSavedSearch', {
                    'name': name,
                    'query': query
                }, 'Failed to delete saved search');
            },
            getLogs: function getLogs(options) {
                var defaults = {
                    pageSize: 100,
                    pageNumber: 1,
                    orderDirection: 'Descending',
                    filterExpression: ''
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('logViewerApiBaseUrl', 'GetLogs', options)), 'Failed to retrieve common log messages');
            },
            canViewLogs: function canViewLogs(startDate, endDate) {
                return request('GET', 'GetCanViewLogs', '?startDate=' + startDate + '&endDate=' + endDate, 'Failed to retrieve state if logs can be viewed');
            }
        };
    }
    angular.module('umbraco.resources').factory('logViewerResource', logViewerResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.macroResource
    * @description Deals with data for macros
    *
    **/
    function macroResource($q, $http, umbRequestHelper) {
        //the factory object returned
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.macroResource#getMacroParameters
     * @methodOf umbraco.resources.macroResource
     *
     * @description
     * Gets the editable macro parameters for the specified macro alias
     *
     * @param {int} macroId The macro id to get parameters for
     *
     */
            getMacroParameters: function getMacroParameters(macroId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('macroRenderingApiBaseUrl', 'GetMacroParameters', [{ macroId: macroId }])), 'Failed to retrieve macro parameters for macro with id  ' + macroId);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.macroResource#getMacroResult
     * @methodOf umbraco.resources.macroResource
     *
     * @description
     * Gets the result of a macro as html to display in the rich text editor or in the Grid
     *
     * @param {int} macroId The macro id to get parameters for
     * @param {int} pageId The current page id
     * @param {Array} macroParamDictionary A dictionary of macro parameters
     *
     */
            getMacroResultAsHtmlForEditor: function getMacroResultAsHtmlForEditor(macroAlias, pageId, macroParamDictionary) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('macroRenderingApiBaseUrl', 'GetMacroResultAsHtmlForEditor'), {
                    macroAlias: macroAlias,
                    pageId: pageId,
                    macroParams: macroParamDictionary
                }), 'Failed to retrieve macro result for macro with alias  ' + macroAlias);
            },
            /**
     *
     * @param {} filename
     * @returns {}
     */
            createPartialViewMacroWithFile: function createPartialViewMacroWithFile(virtualPath, filename) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('macroRenderingApiBaseUrl', 'CreatePartialViewMacroWithFile'), {
                    virtualPath: virtualPath,
                    filename: filename
                }), 'Failed to create macro "' + filename + '"');
            },
            createMacro: function createMacro(name) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('macroApiBaseUrl', 'Create?name=' + name)), 'Failed to create macro "' + name + '"');
            },
            getPartialViews: function getPartialViews() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('macroApiBaseUrl', 'GetPartialViews'), 'Failed to get partial views'));
            },
            getParameterEditors: function getParameterEditors() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('macroApiBaseUrl', 'GetParameterEditors'), 'Failed to get parameter editors'));
            },
            getGroupedParameterEditors: function getGroupedParameterEditors() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('macroApiBaseUrl', 'GetGroupedParameterEditors'), 'Failed to get parameter editors'));
            },
            getParameterEditorByAlias: function getParameterEditorByAlias(alias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('macroApiBaseUrl', 'GetParameterEditorByAlias', { 'alias': alias }), 'Failed to get parameter editor'));
            },
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('macroApiBaseUrl', 'GetById', { 'id': id }), 'Failed to get macro'));
            },
            saveMacro: function saveMacro(macro) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('macroApiBaseUrl', 'Save'), macro));
            },
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('macroApiBaseUrl', 'deleteById', { 'id': id })));
            }
        };
    }
    angular.module('umbraco.resources').factory('macroResource', macroResource);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.mediaResource
 * @description Loads in data for media
 **/
    function mediaResource($q, $http, umbDataFormatter, umbRequestHelper) {
        /** internal method process the saving of data and post processing the result */
        function saveMediaItem(content, action, files) {
            return umbRequestHelper.postSaveContent({
                restApiUrl: umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'PostSave'),
                content: content,
                action: action,
                files: files,
                dataFormatter: function dataFormatter(c, a) {
                    return umbDataFormatter.formatMediaPostData(c, a);
                }
            });
        }
        return {
            getRecycleBin: function getRecycleBin() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'GetRecycleBin')), 'Failed to retrieve data for media recycle bin');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#sort
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Sorts all children below a given parent node id, based on a collection of node-ids
     *
     * ##usage
     * <pre>
     * var ids = [123,34533,2334,23434];
     * mediaResource.sort({ sortedIds: ids })
     *    .then(function() {
     *        $scope.complete = true;
     *    });
     * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.parentId the ID of the parent node
     * @param {Array} options.sortedIds array of node IDs as they should be sorted
     * @returns {Promise} resourcePromise object.
     *
     */
            sort: function sort(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.sortedIds) {
                    throw 'args.sortedIds cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'PostSort'), {
                    parentId: args.parentId,
                    idSortOrder: args.sortedIds
                }), 'Failed to sort media');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#move
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Moves a node underneath a new parentId
     *
     * ##usage
     * <pre>
     * mediaResource.move({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("node was moved");
     *    }, function(err){
     *      alert("node didnt move:" + err.data.Message);
     *    });
     * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.idd the ID of the node to move
     * @param {Int} args.parentId the ID of the parent node to move to
     * @returns {Promise} resourcePromise object.
     *
     */
            move: function move(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'PostMove'), {
                    parentId: args.parentId,
                    id: args.id
                }, { responseType: 'text' }), {
                    error: function error(data) {
                        var errorMsg = 'Failed to move media';
                        if (data.id !== undefined && data.parentId !== undefined) {
                            if (data.id === data.parentId) {
                                errorMsg = 'Media can\'t be moved into itself';
                            }
                        } else if (data.notifications !== undefined) {
                            if (data.notifications.length > 0) {
                                if (data.notifications[0].header.length > 0) {
                                    errorMsg = data.notifications[0].header;
                                }
                                if (data.notifications[0].message.length > 0) {
                                    errorMsg = errorMsg + ': ' + data.notifications[0].message;
                                }
                            }
                        }
                        return { errorMsg: errorMsg };
                    }
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#getById
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Gets a media item with a given id
     *
     * ##usage
     * <pre>
     * mediaResource.getById(1234)
     *    .then(function(media) {
     *        var myMedia = media;
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {Int} id id of media item to return
     * @returns {Promise} resourcePromise object containing the media item.
     *
     */
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to retrieve data for media id ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#deleteById
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Deletes a media item with a given id
     *
     * ##usage
     * <pre>
     * mediaResource.deleteById(1234)
     *    .then(function() {
     *        alert('its gone!');
     *    });
     * </pre>
     *
     * @param {Int} id id of media item to delete
     * @returns {Promise} resourcePromise object.
     *
     */
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete item ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#getByIds
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Gets an array of media items, given a collection of ids
     *
     * ##usage
     * <pre>
     * mediaResource.getByIds( [1234,2526,28262])
     *    .then(function(mediaArray) {
     *        var myDoc = contentArray;
     *        alert('they are here!');
     *    });
     * </pre>
     *
     * @param {Array} ids ids of media items to return as an array
     * @returns {Promise} resourcePromise object containing the media items array.
     *
     */
            getByIds: function getByIds(ids) {
                var idQuery = '';
                ids.forEach(function (id) {
                    return idQuery += 'ids='.concat(id, '&');
                });
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'GetByIds', idQuery)), 'Failed to retrieve data for media ids ' + ids);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#getScaffold
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Returns a scaffold of an empty media item, given the id of the media item to place it underneath and the media type alias.
     *
     * - Parent Id must be provided so umbraco knows where to store the media
     * - Media Type alias must be provided so umbraco knows which properties to put on the media scaffold
     *
     * The scaffold is used to build editors for media that has not yet been populated with data.
     *
     * ##usage
     * <pre>
     * mediaResource.getScaffold(1234, 'folder')
     *    .then(function(scaffold) {
     *        var myDoc = scaffold;
     *        myDoc.name = "My new media item";
     *
     *        mediaResource.save(myDoc, true)
     *            .then(function(media){
     *                alert("Retrieved, updated and saved again");
     *            });
     *    });
     * </pre>
     *
     * @param {Int} parentId id of media item to return
     * @param {String} alias mediatype alias to base the scaffold on
     * @returns {Promise} resourcePromise object containing the media scaffold.
     *
     */
            getScaffold: function getScaffold(parentId, alias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'GetEmpty', [
                    { contentTypeAlias: alias },
                    { parentId: parentId }
                ])), 'Failed to retrieve data for empty media item type ' + alias);
            },
            rootMedia: function rootMedia() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'GetRootMedia')), 'Failed to retrieve data for root media');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#getChildren
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Gets children of a media item with a given id
     *
     * ##usage
     * <pre>
     * mediaResource.getChildren(1234, {pageSize: 10, pageNumber: 2})
     *    .then(function(contentArray) {
     *        var children = contentArray;
     *        alert('they are here!');
     *    });
     * </pre>
     *
     * @param {Int} parentid id of content item to return children of
     * @param {Object} options optional options object
     * @param {Int} options.pageSize if paging data, number of nodes per page, default = 0
     * @param {Int} options.pageNumber if paging data, current page index, default = 0
     * @param {String} options.filter if provided, query will only return those with names matching the filter
     * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
     * @param {String} options.orderBy property to order items by, default: `SortOrder`
     * @returns {Promise} resourcePromise object containing an array of content items.
     *
     */
            getChildren: function getChildren(parentId, options) {
                var defaults = {
                    pageSize: 0,
                    pageNumber: 0,
                    filter: '',
                    orderDirection: 'Ascending',
                    orderBy: 'SortOrder',
                    orderBySystemField: true
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                //change asc/desct
                if (options.orderDirection === 'asc') {
                    options.orderDirection = 'Ascending';
                } else if (options.orderDirection === 'desc') {
                    options.orderDirection = 'Descending';
                }
                //converts the value to a js bool
                function toBool(v) {
                    if (Utilities.isNumber(v)) {
                        return v > 0;
                    }
                    if (Utilities.isString(v)) {
                        return v === 'true';
                    }
                    if (typeof v === 'boolean') {
                        return v;
                    }
                    return false;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'GetChildren', [
                    { id: parentId },
                    { pageNumber: options.pageNumber },
                    { pageSize: options.pageSize },
                    { orderBy: options.orderBy },
                    { orderDirection: options.orderDirection },
                    { orderBySystemField: toBool(options.orderBySystemField) },
                    { filter: options.filter }
                ])), 'Failed to retrieve children for media item ' + parentId);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#save
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Saves changes made to a media item, if the media item is new, the isNew paramater must be passed to force creation
     * if the media item needs to have files attached, they must be provided as the files param and passed separately
     *
     *
     * ##usage
     * <pre>
     * mediaResource.getById(1234)
     *    .then(function(media) {
     *          media.name = "I want a new name!";
     *          mediaResource.save(media, false)
     *            .then(function(media){
     *                alert("Retrieved, updated and saved again");
     *            });
     *    });
     * </pre>
     *
     * @param {Object} media The media item object with changes applied
     * @param {Bool} isNew set to true to create a new item or to update an existing
     * @param {Array} files collection of files for the media item
     * @returns {Promise} resourcePromise object containing the saved media item.
     *
     */
            save: function save(media, isNew, files) {
                return saveMediaItem(media, 'save' + (isNew ? 'New' : ''), files);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#addFolder
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Shorthand for adding a media item of the type "Folder" under a given parent ID
     *
     * ##usage
     * <pre>
     * mediaResource.addFolder("My gallery", 1234)
     *    .then(function(folder) {
     *        alert('New folder');
     *    });
     * </pre>
     *
     * @param {string} name Name of the folder to create
     * @param {int} parentId Id of the media item to create the folder underneath
     * @returns {Promise} resourcePromise object.
     *
     */
            addFolder: function addFolder(name, parentId) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'PostAddFolder'), {
                    name: name,
                    parentId: parentId
                }), 'Failed to add folder');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#getChildFolders
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Retrieves all media children with types used as folders.
     * Uses the convention of looking for media items with mediaTypes ending in
     * *Folder so will match "Folder", "bannerFolder", "secureFolder" etc,
     *
      * NOTE: This will return a page of max 500 folders, if more is required it needs to be paged
      *       and then folders are in the .items property of the returned promise data
     *
     * ##usage
     * <pre>
     * mediaResource.getChildFolders(1234)
      *    .then(function(page) {
     *        alert('folders');
     *    });
     * </pre>
     *
     * @param {int} parentId Id of the media item to query for child folders
     * @returns {Promise} resourcePromise object.
     *
     */
            getChildFolders: function getChildFolders(parentId) {
                if (!parentId) {
                    parentId = -1;
                }
                //NOTE: This will return a max of 500 folders, if more is required it needs to be paged
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'GetChildFolders', {
                    id: parentId,
                    pageNumber: 1,
                    pageSize: 500
                })), 'Failed to retrieve child folders for media item ' + parentId);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#emptyRecycleBin
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Empties the media recycle bin
     *
     * ##usage
     * <pre>
     * mediaResource.emptyRecycleBin()
     *    .then(function() {
     *        alert('its empty!');
     *    });
     * </pre>
     *
     * @returns {Promise} resourcePromise object.
     *
     */
            emptyRecycleBin: function emptyRecycleBin() {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'EmptyRecycleBin')), 'Failed to empty the recycle bin');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaResource#search
     * @methodOf umbraco.resources.mediaResource
     *
     * @description
     * Paginated search for media items starting on the supplied nodeId
     *
     * ##usage
     * <pre>
     * mediaResource.search("my search", 1, 100, -1)
     *    .then(function(searchResult) {
     *        alert('it's here!');
     *    });
     * </pre>
     *
     * @param {string} query The search query
     * @param {int} pageNumber The page number
     * @param {int} pageSize The number of media items on a page
     * @param {int} searchFrom NodeId to search from (-1 for root)
     * @returns {Promise} resourcePromise object.
     *
     */
            search: function search(query, pageNumber, pageSize, searchFrom) {
                var args = [
                    { 'query': query },
                    { 'pageNumber': pageNumber },
                    { 'pageSize': pageSize },
                    { 'searchFrom': searchFrom }
                ];
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'Search', args)), 'Failed to retrieve media items for search: ' + query);
            },
            getPagedReferences: function getPagedReferences(id, options) {
                var defaults = {
                    pageSize: 25,
                    pageNumber: 1,
                    entityType: 'DOCUMENT'
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaApiBaseUrl', 'GetPagedReferences', {
                    id: id,
                    entityType: options.entityType,
                    pageNumber: options.pageNumber,
                    pageSize: options.pageSize
                })), 'Failed to retrieve usages for media of id ' + id);
            }
        };
    }
    angular.module('umbraco.resources').factory('mediaResource', mediaResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.mediaTypeResource
    * @description Loads in data for media types
    **/
    function mediaTypeResource($q, $http, umbRequestHelper, umbDataFormatter, localizationService) {
        return {
            getCount: function getCount() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'GetCount')), 'Failed to retrieve count');
            },
            getAvailableCompositeContentTypes: function getAvailableCompositeContentTypes(contentTypeId, filterContentTypes, filterPropertyTypes) {
                if (!filterContentTypes) {
                    filterContentTypes = [];
                }
                if (!filterPropertyTypes) {
                    filterPropertyTypes = [];
                }
                var query = {
                    contentTypeId: contentTypeId,
                    filterContentTypes: filterContentTypes,
                    filterPropertyTypes: filterPropertyTypes
                };
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'GetAvailableCompositeMediaTypes'), query), 'Failed to retrieve data for content type id ' + contentTypeId);
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.mediaTypeResource#getWhereCompositionIsUsedInContentTypes
    * @methodOf umbraco.resources.mediaTypeResource
    *
    * @description
    * Returns a list of media types which use a specific composition with a given id
    *
    * ##usage
    * <pre>
    * mediaTypeResource.getWhereCompositionIsUsedInContentTypes(1234)
    *    .then(function(mediaTypeList) {
    *        console.log(mediaTypeList);
    *    });
    * </pre>
    * @param {Int} contentTypeId id of the composition content type to retrieve the list of the media types where it has been used
    * @returns {Promise} resourcePromise object.
    *
    */
            getWhereCompositionIsUsedInContentTypes: function getWhereCompositionIsUsedInContentTypes(contentTypeId) {
                var query = { contentTypeId: contentTypeId };
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'GetWhereCompositionIsUsedInContentTypes'), query), 'Failed to retrieve data for content type id ' + contentTypeId);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaTypeResource#getAllowedTypes
     * @methodOf umbraco.resources.mediaTypeResource
     *
     * @description
     * Returns a list of allowed media types underneath a media item with a given ID
     *
     * ##usage
     * <pre>
     * mediaTypeResource.getAllowedTypes(1234)
     *    .then(function(array) {
     *        $scope.type = type;
     *    });
     * </pre>
     * @param {Int} mediaId id of the media item to retrive allowed child types for
     * @returns {Promise} resourcePromise object.
     *
     */
            getAllowedTypes: function getAllowedTypes(mediaId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'GetAllowedChildren', [{ contentId: mediaId }])), 'Failed to retrieve allowed types for media id ' + mediaId);
            },
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to retrieve content type');
            },
            getAll: function getAll() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'GetAll')), 'Failed to retrieve all content types');
            },
            getScaffold: function getScaffold(parentId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'GetEmpty', { parentId: parentId })), 'Failed to retrieve content type scaffold');
            },
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to retrieve content type');
            },
            deleteContainerById: function deleteContainerById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'DeleteContainer', [{ id: id }])), 'Failed to delete content type contaier');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaTypeResource#save
     * @methodOf umbraco.resources.mediaTypeResource
     *
     * @description
     * Saves or update a media type
     *
     * @param {Object} content data type object to create/update
     * @returns {Promise} resourcePromise object.
     *
     */
            save: function save(contentType) {
                var saveModel = umbDataFormatter.formatContentTypePostData(contentType);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'PostSave'), saveModel), 'Failed to save data for content type id ' + contentType.id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.mediaTypeResource#move
     * @methodOf umbraco.resources.mediaTypeResource
     *
     * @description
     * Moves a node underneath a new parentId
     *
     * ##usage
     * <pre>
     * mediaTypeResource.move({ parentId: 1244, id: 123 })
     *    .then(function() {
     *        alert("node was moved");
     *    }, function(err){
     *      alert("node didnt move:" + err.data.Message);
     *    });
     * </pre>
     * @param {Object} args arguments object
     * @param {Int} args.idd the ID of the node to move
     * @param {Int} args.parentId the ID of the parent node to move to
     * @returns {Promise} resourcePromise object.
     *
     */
            move: function move(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                var promise = localizationService.localize('mediaType_moveFailed');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'PostMove'), {
                    parentId: args.parentId,
                    id: args.id
                }, { responseType: 'text' }), promise);
            },
            copy: function copy(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                var promise = localizationService.localize('mediaType_copyFailed');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'PostCopy'), {
                    parentId: args.parentId,
                    id: args.id
                }, { responseType: 'text' }), promise);
            },
            createContainer: function createContainer(parentId, name) {
                var promise = localizationService.localize('media_createFolderFailed', [parentId]);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'PostCreateContainer', {
                    parentId: parentId,
                    name: encodeURIComponent(name)
                })), promise);
            },
            renameContainer: function renameContainer(id, name) {
                var promise = localizationService.localize('media_renameFolderFailed', [id]);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('mediaTypeApiBaseUrl', 'PostRenameContainer', {
                    id: id,
                    name: name
                })), promise);
            }
        };
    }
    angular.module('umbraco.resources').factory('mediaTypeResource', mediaTypeResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.memberResource
    * @description Loads in data for members
    **/
    function memberResource($q, $http, umbDataFormatter, umbRequestHelper) {
        /** internal method process the saving of data and post processing the result */
        function saveMember(content, action, files) {
            return umbRequestHelper.postSaveContent({
                restApiUrl: umbRequestHelper.getApiUrl('memberApiBaseUrl', 'PostSave'),
                content: content,
                action: action,
                files: files,
                dataFormatter: function dataFormatter(c, a) {
                    return umbDataFormatter.formatMemberPostData(c, a);
                }
            });
        }
        return {
            getPagedResults: function getPagedResults(memberTypeAlias, options) {
                if (memberTypeAlias === 'all-members') {
                    memberTypeAlias = null;
                }
                var defaults = {
                    pageSize: 25,
                    pageNumber: 1,
                    filter: '',
                    orderDirection: 'Ascending',
                    orderBy: 'LoginName',
                    orderBySystemField: true
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                //change asc/desct
                if (options.orderDirection === 'asc') {
                    options.orderDirection = 'Ascending';
                } else if (options.orderDirection === 'desc') {
                    options.orderDirection = 'Descending';
                }
                //converts the value to a js bool
                function toBool(v) {
                    if (Utilities.isNumber(v)) {
                        return v > 0;
                    }
                    if (Utilities.isString(v)) {
                        return v === 'true';
                    }
                    if (typeof v === 'boolean') {
                        return v;
                    }
                    return false;
                }
                var params = [
                    { pageNumber: options.pageNumber },
                    { pageSize: options.pageSize },
                    { orderBy: options.orderBy },
                    { orderDirection: options.orderDirection },
                    { orderBySystemField: toBool(options.orderBySystemField) },
                    { filter: options.filter }
                ];
                if (memberTypeAlias != null) {
                    params.push({ memberTypeAlias: memberTypeAlias });
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberApiBaseUrl', 'GetPagedResults', params)), 'Failed to retrieve member paged result');
            },
            getListNode: function getListNode(listName) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberApiBaseUrl', 'GetListNodeDisplay', [{ listName: listName }])), 'Failed to retrieve data for member list ' + listName);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.memberResource#getByKey
      * @methodOf umbraco.resources.memberResource
      *
      * @description
      * Gets a member item with a given key
      *
      * ##usage
      * <pre>
      * memberResource.getByKey("0000-0000-000-00000-000")
      *    .then(function(member) {
      *        var mymember = member; 
      *        alert('its here!');
      *    });
      * </pre> 
      * 
      * @param {Guid} key key of member item to return        
      * @returns {Promise} resourcePromise object containing the member item.
      *
      */
            getByKey: function getByKey(key) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberApiBaseUrl', 'GetByKey', [{ key: key }])), 'Failed to retrieve data for member id ' + key);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.memberResource#deleteByKey
      * @methodOf umbraco.resources.memberResource
      *
      * @description
      * Deletes a member item with a given key
      *
      * ##usage
      * <pre>
      * memberResource.deleteByKey("0000-0000-000-00000-000")
      *    .then(function() {
      *        alert('its gone!');
      *    });
      * </pre> 
      * 
      * @param {Guid} key id of member item to delete        
      * @returns {Promise} resourcePromise object.
      *
      */
            deleteByKey: function deleteByKey(key) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('memberApiBaseUrl', 'DeleteByKey', [{ key: key }])), 'Failed to delete item ' + key);
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.memberResource#getScaffold
      * @methodOf umbraco.resources.memberResource
      *
      * @description
      * Returns a scaffold of an empty member item, given the id of the member item to place it underneath and the member type alias.
      *         
      * - Member Type alias must be provided so umbraco knows which properties to put on the member scaffold 
      * 
      * The scaffold is used to build editors for member that has not yet been populated with data.
      * 
      * ##usage
      * <pre>
      * memberResource.getScaffold('client')
      *    .then(function(scaffold) {
      *        var myDoc = scaffold;
      *        myDoc.name = "My new member item"; 
      *
      *        memberResource.save(myDoc, true)
      *            .then(function(member){
      *                alert("Retrieved, updated and saved again");
      *            });
      *    });
      * </pre> 
      * 
      * @param {String} alias membertype alias to base the scaffold on        
      * @returns {Promise} resourcePromise object containing the member scaffold.
      *
      */
            getScaffold: function getScaffold(alias) {
                if (alias) {
                    return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberApiBaseUrl', 'GetEmpty', [{ contentTypeAlias: alias }])), 'Failed to retrieve data for empty member item type ' + alias);
                } else {
                    return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberApiBaseUrl', 'GetEmpty')), 'Failed to retrieve data for empty member item type ' + alias);
                }
            },
            /**
      * @ngdoc method
      * @name umbraco.resources.memberResource#save
      * @methodOf umbraco.resources.memberResource
      *
      * @description
      * Saves changes made to a member, if the member is new, the isNew paramater must be passed to force creation
      * if the member needs to have files attached, they must be provided as the files param and passed separately 
      * 
      * 
      * ##usage
      * <pre>
      * memberResource.getBykey("23234-sd8djsd-3h8d3j-sdh8d")
      *    .then(function(member) {
      *          member.name = "Bob";
      *          memberResource.save(member, false)
      *            .then(function(member){
      *                alert("Retrieved, updated and saved again");
      *            });
      *    });
      * </pre> 
      * 
      * @param {Object} media The member item object with changes applied
      * @param {Bool} isNew set to true to create a new item or to update an existing 
      * @param {Array} files collection of files for the media item      
      * @returns {Promise} resourcePromise object containing the saved media item.
      *
      */
            save: function save(member, isNew, files) {
                return saveMember(member, 'save' + (isNew ? 'New' : ''), files);
            }
        };
    }
    angular.module('umbraco.resources').factory('memberResource', memberResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.memberGroupResource
    * @description Loads in data for member groups
    **/
    function memberGroupResource($q, $http, umbRequestHelper) {
        return {
            //return all member types
            getGroups: function getGroups() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberGroupApiBaseUrl', 'GetAllGroups')), 'Failed to retrieve data for member groups');
            },
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberGroupApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to retrieve member group');
            },
            getByIds: function getByIds(ids) {
                var idQuery = '';
                ids.forEach(function (id) {
                    return idQuery += 'ids='.concat(id, '&');
                });
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberGroupApiBaseUrl', 'GetByIds', idQuery)), 'Failed to retrieve member group');
            },
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('memberGroupApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete member group');
            },
            getScaffold: function getScaffold() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberGroupApiBaseUrl', 'GetEmpty')), 'Failed to retrieve data for member group');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.memberGroupResource#save
     * @methodOf umbraco.resources.memberGroupResource
     *
     * @description
     * Saves or update a member group
     *
     * @param {Object} member group object to create/update
     * @returns {Promise} resourcePromise object.
     *
     */
            save: function save(memberGroup) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('memberGroupApiBaseUrl', 'PostSave'), memberGroup), 'Failed to save data for member group, id: ' + memberGroup.id);
            }
        };
    }
    angular.module('umbraco.resources').factory('memberGroupResource', memberGroupResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.memberTypeResource
    * @description Loads in data for member types
    **/
    function memberTypeResource($q, $http, umbRequestHelper, umbDataFormatter, localizationService) {
        return {
            getAvailableCompositeContentTypes: function getAvailableCompositeContentTypes(contentTypeId, filterContentTypes, filterPropertyTypes) {
                if (!filterContentTypes) {
                    filterContentTypes = [];
                }
                if (!filterPropertyTypes) {
                    filterPropertyTypes = [];
                }
                var query = '';
                filterContentTypes.forEach(function (fct) {
                    return query += 'filterContentTypes='.concat(fct, '&');
                });
                // if filterContentTypes array is empty we need a empty variable in the querystring otherwise the service returns a error
                if (filterContentTypes.length === 0) {
                    query += 'filterContentTypes=&';
                }
                filterPropertyTypes.forEach(function (fpt) {
                    return query += 'filterPropertyTypes='.concat(fpt, '&');
                });
                // if filterPropertyTypes array is empty we need a empty variable in the querystring otherwise the service returns a error
                if (filterPropertyTypes.length === 0) {
                    query += 'filterPropertyTypes=&';
                }
                query += 'contentTypeId=' + contentTypeId;
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberTypeApiBaseUrl', 'GetAvailableCompositeMemberTypes', query)), 'Failed to retrieve data for content type id ' + contentTypeId);
            },
            //return all member types
            getTypes: function getTypes() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberTypeApiBaseUrl', 'GetAllTypes')), 'Failed to retrieve data for member types id');
            },
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberTypeApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to retrieve content type');
            },
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('memberTypeApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete member type');
            },
            getScaffold: function getScaffold() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('memberTypeApiBaseUrl', 'GetEmpty')), 'Failed to retrieve content type scaffold');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.memberTypeResource#save
     * @methodOf umbraco.resources.memberTypeResource
     *
     * @description
     * Saves or update a member type
     *
     * @param {Object} content data type object to create/update
     * @returns {Promise} resourcePromise object.
     *
     */
            save: function save(contentType) {
                var saveModel = umbDataFormatter.formatContentTypePostData(contentType);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('memberTypeApiBaseUrl', 'PostSave'), saveModel), 'Failed to save data for member type id ' + contentType.id);
            },
            copy: function copy(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.parentId) {
                    throw 'args.parentId cannot be null';
                }
                if (!args.id) {
                    throw 'args.id cannot be null';
                }
                var promise = localizationService.localize('memberType_copyFailed');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('memberTypeApiBaseUrl', 'PostCopy'), {
                    parentId: args.parentId,
                    id: args.id
                }, { responseType: 'text' }), promise);
            }
        };
    }
    angular.module('umbraco.resources').factory('memberTypeResource', memberTypeResource);
    'use strict';
    /**
* @ngdoc service
* @name umbraco.resources.modelsBuilderManagementResource
* @description Resources to get information on modelsbuilder status and build models
**/
    function modelsBuilderManagementResource($q, $http, umbRequestHelper) {
        return {
            /**
    * @ngdoc method
    * @name umbraco.resources.modelsBuilderManagementResource#getModelsOutOfDateStatus
    * @methodOf umbraco.resources.modelsBuilderManagementResource
    *
    * @description
    * Gets the status of modelsbuilder 
    *
    * ##usage
    * <pre>
    * modelsBuilderManagementResource.getModelsOutOfDateStatus()
    *  .then(function() {
    *        Do stuff...*
    * });
    * </pre>
    * 
    * @returns {Promise} resourcePromise object.
    *
    */
            getModelsOutOfDateStatus: function getModelsOutOfDateStatus() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('modelsBuilderBaseUrl', 'GetModelsOutOfDateStatus')), 'Failed to get models out-of-date status');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.modelsBuilderManagementResource#buildModels
    * @methodOf umbraco.resources.modelsBuilderManagementResource
    *
    * @description
    * Builds the models
    *
    * ##usage
    * <pre>
    * modelsBuilderManagementResource.buildModels()
    *  .then(function() {
    *        Do stuff...*
    * });
    * </pre>
    *
    * @returns {Promise} resourcePromise object.
    *
    */
            buildModels: function buildModels() {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('modelsBuilderBaseUrl', 'BuildModels')), 'Failed to build models');
            },
            /**
    * @ngdoc method
    * @name umbraco.resources.modelsBuilderManagementResource#getDashboard
    * @methodOf umbraco.resources.modelsBuilderManagementResource
    *
    * @description
    * Gets the modelsbuilder dashboard
    *
    * ##usage
    * <pre>
    * modelsBuilderManagementResource.getDashboard()
    *  .then(function() {
    *        Do stuff...*
    * });
    * </pre>
    *
    * @returns {Promise} resourcePromise object.
    *
    */
            getDashboard: function getDashboard() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('modelsBuilderBaseUrl', 'GetDashboard')), 'Failed to get dashboard');
            }
        };
    }
    angular.module('umbraco.resources').factory('modelsBuilderManagementResource', modelsBuilderManagementResource);
    'use strict';
    angular.module('umbraco.resources').factory('Umbraco.PropertyEditors.NestedContent.Resources', function ($q, $http, umbRequestHelper) {
        return {
            getContentTypes: function getContentTypes() {
                var url = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/backoffice/UmbracoApi/NestedContent/GetContentTypes';
                return umbRequestHelper.resourcePromise($http.get(url), 'Failed to retrieve content types');
            }
        };
    });
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.ourPackageRepositoryResource
    * @description handles data for package installations
    **/
    function ourPackageRepositoryResource($q, $http, umbDataFormatter, umbRequestHelper) {
        var baseurl = Umbraco.Sys.ServerVariables.umbracoUrls.packagesRestApiBaseUrl;
        return {
            getDetails: function getDetails(packageId) {
                return umbRequestHelper.resourcePromise($http.get(baseurl + '/' + packageId + '?version=' + Umbraco.Sys.ServerVariables.application.version), 'Failed to get package details');
            },
            getCategories: function getCategories() {
                return umbRequestHelper.resourcePromise($http.get(baseurl), 'Failed to query packages');
            },
            getPopular: function getPopular(maxResults, category) {
                if (maxResults === undefined) {
                    maxResults = 10;
                }
                if (category === undefined) {
                    category = '';
                }
                return umbRequestHelper.resourcePromise($http.get(baseurl + '?pageIndex=0&pageSize=' + maxResults + '&category=' + category + '&order=Popular&version=' + Umbraco.Sys.ServerVariables.application.version), 'Failed to query packages');
            },
            search: function search(pageIndex, pageSize, orderBy, category, query, canceler) {
                var httpConfig = {};
                if (canceler) {
                    httpConfig['timeout'] = canceler;
                }
                if (category === undefined) {
                    category = '';
                }
                if (query === undefined) {
                    query = '';
                }
                //order by score if there is nothing set
                var order = !orderBy ? '&order=Default' : '&order=' + orderBy;
                return umbRequestHelper.resourcePromise($http.get(baseurl + '?pageIndex=' + pageIndex + '&pageSize=' + pageSize + '&category=' + category + '&query=' + query + order + '&version=' + Umbraco.Sys.ServerVariables.application.version), httpConfig, 'Failed to query packages');
            }
        };
    }
    angular.module('umbraco.resources').factory('ourPackageRepositoryResource', ourPackageRepositoryResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.packageInstallResource
    * @description handles data for package installations
    **/
    function packageResource($q, $http, umbDataFormatter, umbRequestHelper) {
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.packageInstallResource#getInstalled
     * @methodOf umbraco.resources.packageInstallResource
     *
     * @description
     * Gets a list of installed packages       
     */
            getInstalled: function getInstalled() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('packageApiBaseUrl', 'GetInstalled')), 'Failed to get installed packages');
            },
            validateInstalled: function validateInstalled(name, version) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageInstallApiBaseUrl', 'ValidateInstalled', {
                    name: name,
                    version: version
                })), 'Failed to validate package ' + name);
            },
            uninstall: function uninstall(packageId) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageInstallApiBaseUrl', 'Uninstall', { packageId: packageId })), 'Failed to uninstall package');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.packageInstallResource#fetchPackage
     * @methodOf umbraco.resources.packageInstallResource
     *
     * @description
     * Downloads a package file from our.umbraco.com to the website server.
     * 
     * ##usage
     * <pre>
     * packageResource.download("guid-guid-guid-guid")
     *    .then(function(path) {
     *        alert('downloaded');
     *    });
     * </pre> 
     *  
     * @param {String} the unique package ID
     * @returns {String} path to the downloaded zip file.
     *
     */
            fetch: function fetch(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('packageInstallApiBaseUrl', 'Fetch', [{ packageGuid: id }])), 'Failed to download package with guid ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.packageInstallResource#createmanifest
     * @methodOf umbraco.resources.packageInstallResource
     *
     * @description
     * Creates a package manifest for a given folder of files. 
     * This manifest keeps track of all installed files and data items
     * so a package can be uninstalled at a later time.
     * After creating a manifest, you can use the ID to install files and data.
     * 
     * ##usage
     * <pre>
     * packageResource.createManifest("packages/id-of-install-file")
     *    .then(function(summary) {
     *        alert('unzipped');
     *    });
     * </pre> 
     *  
     * @param {String} folder the path to the temporary folder containing files
     * @returns {Int} the ID assigned to the saved package manifest
     *
     */
            import: function _import(umbPackage) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageInstallApiBaseUrl', 'Import'), umbPackage), 'Failed to install package. Error during the step "Import" ');
            },
            installFiles: function installFiles(umbPackage) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageInstallApiBaseUrl', 'InstallFiles'), umbPackage), 'Failed to install package. Error during the step "InstallFiles" ');
            },
            checkRestart: function checkRestart(umbPackage) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageInstallApiBaseUrl', 'CheckRestart'), umbPackage), 'Failed to install package. Error during the step "CheckRestart" ');
            },
            installData: function installData(umbPackage) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageInstallApiBaseUrl', 'InstallData'), umbPackage), 'Failed to install package. Error during the step "InstallData" ');
            },
            cleanUp: function cleanUp(umbPackage) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageInstallApiBaseUrl', 'CleanUp'), umbPackage), 'Failed to install package. Error during the step "CleanUp" ');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.packageInstallResource#getCreated
     * @methodOf umbraco.resources.packageInstallResource
     *
     * @description
     * Gets a list of created packages       
     */
            getAllCreated: function getAllCreated() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('packageApiBaseUrl', 'GetCreatedPackages')), 'Failed to get created packages');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.packageInstallResource#getCreatedById
     * @methodOf umbraco.resources.packageInstallResource
     *
     * @description
     * Gets a created package by id       
     */
            getCreatedById: function getCreatedById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('packageApiBaseUrl', 'GetCreatedPackageById', { id: id })), 'Failed to get package');
            },
            getInstalledById: function getInstalledById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('packageApiBaseUrl', 'GetInstalledPackageById', { id: id })), 'Failed to get package');
            },
            getEmpty: function getEmpty() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('packageApiBaseUrl', 'getEmpty')), 'Failed to get scaffold');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.packageInstallResource#savePackage
     * @methodOf umbraco.resources.packageInstallResource
     *
     * @description
     * Creates or updates a package
     */
            savePackage: function savePackage(umbPackage) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageApiBaseUrl', 'PostSavePackage'), umbPackage), 'Failed to create package');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.packageInstallResource#deleteCreatedPackage
     * @methodOf umbraco.resources.packageInstallResource
     *
     * @description
     * Detes a created package
     */
            deleteCreatedPackage: function deleteCreatedPackage(packageId) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('packageApiBaseUrl', 'DeleteCreatedPackage', { packageId: packageId })), 'Failed to delete package ' + packageId);
            }
        };
    }
    angular.module('umbraco.resources').factory('packageResource', packageResource);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.redirectUrlResource
 * @function
 *
 * @description
 * Used by the redirect url dashboard to get urls and send requests to remove redirects.
 */
    (function () {
        'use strict';
        function redirectUrlsResource($http, umbRequestHelper) {
            /**
     * @ngdoc function
     * @name umbraco.resources.redirectUrlResource#searchRedirectUrls
     * @methodOf umbraco.resources.redirectUrlResource
     * @function
     *
     * @description
     * Called to search redirects
     * ##usage
     * <pre>
     * redirectUrlsResource.searchRedirectUrls("", 0, 20)
     *    .then(function(response) {
     *
     *    });
     * </pre>
     * @param {String} searchTerm Searh term
     * @param {Int} pageIndex index of the page to retrive items from
     * @param {Int} pageSize The number of items on a page
     */
            function searchRedirectUrls(searchTerm, pageIndex, pageSize) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('redirectUrlManagementApiBaseUrl', 'SearchRedirectUrls', {
                    searchTerm: searchTerm,
                    page: pageIndex,
                    pageSize: pageSize
                })), 'Failed to retrieve data for searching redirect urls');
            }
            /**
    * @ngdoc function
    * @name umbraco.resources.redirectUrlResource#getRedirectsForContentItem
    * @methodOf umbraco.resources.redirectUrlResource
    * @function
    *
    * @description
    * Used to retrieve RedirectUrls for a specific item of content for Information tab
    * ##usage
    * <pre>
    * redirectUrlsResource.getRedirectsForContentItem("udi:123456")
    *    .then(function(response) {
    *
    *    });
    * </pre>
    * @param {String} contentUdi identifier for the content item to retrieve redirects for
    */
            function getRedirectsForContentItem(contentUdi) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('redirectUrlManagementApiBaseUrl', 'RedirectUrlsForContentItem', { contentUdi: contentUdi })), 'Failed to retrieve redirects for content: ' + contentUdi);
            }
            function getEnableState() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('redirectUrlManagementApiBaseUrl', 'GetEnableState')), 'Failed to retrieve data to check if the 301 redirect is enabled');
            }
            /**
     * @ngdoc function
     * @name umbraco.resources.redirectUrlResource#deleteRedirectUrl
     * @methodOf umbraco.resources.redirectUrlResource
     * @function
     *
     * @description
     * Called to delete a redirect
     * ##usage
     * <pre>
     * redirectUrlsResource.deleteRedirectUrl(1234)
     *    .then(function() {
     *
     *    });
     * </pre>
     * @param {Int} id Id of the redirect
     */
            function deleteRedirectUrl(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('redirectUrlManagementApiBaseUrl', 'DeleteRedirectUrl', { id: id })), 'Failed to remove redirect');
            }
            /**
     * @ngdoc function
     * @name umbraco.resources.redirectUrlResource#toggleUrlTracker
     * @methodOf umbraco.resources.redirectUrlResource
     * @function
     *
     * @description
     * Called to enable or disable redirect url tracker
     * ##usage
     * <pre>
     * redirectUrlsResource.toggleUrlTracker(true)
     *    .then(function() {
     *
     *    });
     * </pre>
     * @param {Bool} disable true/false to disable/enable the url tracker
     */
            function toggleUrlTracker(disable) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('redirectUrlManagementApiBaseUrl', 'ToggleUrlTracker', { disable: disable })), 'Failed to toggle redirect url tracker');
            }
            var resource = {
                searchRedirectUrls: searchRedirectUrls,
                deleteRedirectUrl: deleteRedirectUrl,
                toggleUrlTracker: toggleUrlTracker,
                getEnableState: getEnableState,
                getRedirectsForContentItem: getRedirectsForContentItem
            };
            return resource;
        }
        angular.module('umbraco.resources').factory('redirectUrlsResource', redirectUrlsResource);
    }());
    'use strict';
    /**
  * @ngdoc service
  * @name umbraco.resources.relationResource
  * @description Handles loading of relation data
  **/
    function relationResource($q, $http, umbRequestHelper) {
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.relationResource#getByChildId
     * @methodOf umbraco.resources.relationResource
     *
     * @description
     * Retrieves the relation data for a given child ID
     * 
     * @param {int} id of the child item
     * @param {string} alias of the relation type
     * @returns {Promise} resourcePromise object containing the relations array.
     *
     */
            getByChildId: function getByChildId(id, alias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('relationApiBaseUrl', 'GetByChildId', {
                    childId: id,
                    relationTypeAlias: alias
                })), 'Failed to get relation by child ID ' + id + ' and type of ' + alias);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.relationResource#deleteById
     * @methodOf umbraco.resources.relationResource
     *
     * @description
     * Deletes a relation item with a given id
     *
     * ##usage
     * <pre>
     * relationResource.deleteById(1234)
     *    .then(function() {
     *        alert('its gone!');
     *    });
     * </pre> 
     * 
     * @param {Int} id id of relation item to delete
     * @returns {Promise} resourcePromise object.
     *
     */
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('relationApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete item ' + id);
            }
        };
    }
    angular.module('umbraco.resources').factory('relationResource', relationResource);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.relationTypeResource
 * @description Loads in data for relation types.
 */
    function relationTypeResource($q, $http, umbRequestHelper, umbDataFormatter) {
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.relationTypeResource#getById
     * @methodOf umbraco.resources.relationTypeResource
     *
     * @description
     * Gets a relation type with a given ID.
     *
     * ##usage
     * <pre>
     * relationTypeResource.getById(1234)
     *    .then(function() {
     *        alert('Found it!');
     *    });
     * </pre>
     *
     * @param {Int} id of the relation type to get.
     * @returns {Promise} resourcePromise containing relation type data.
     */
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('relationTypeApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to get item ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.relationTypeResource#getRelationObjectTypes
     * @methodOf umbraco.resources.relationTypeResource
     *
     * @description
     * Gets a list of Umbraco object types which can be associated with a relation.
     *
     * @returns {Object} A collection of Umbraco object types.
     */
            getRelationObjectTypes: function getRelationObjectTypes() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('relationTypeApiBaseUrl', 'GetRelationObjectTypes')), 'Failed to get object types');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.relationTypeResource#save
     * @methodOf umbraco.resources.relationTypeResource
     *
     * @description
     * Updates a relation type.
     *
     * @param {Object} relationType The relation type object to update.
     * @returns {Promise} A resourcePromise object.
     */
            save: function save(relationType) {
                var saveModel = umbDataFormatter.formatRelationTypePostData(relationType);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('relationTypeApiBaseUrl', 'PostSave'), saveModel), 'Failed to save data for relation type ID' + relationType.id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.relationTypeResource#create
     * @methodOf umbraco.resources.relationTypeResource
     *
     * @description
     * Creates a new relation type.
     *
     * @param {Object} relationType The relation type object to create.
     * @returns {Promise} A resourcePromise object.
     */
            create: function create(relationType) {
                var createModel = umbDataFormatter.formatRelationTypePostData(relationType);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('relationTypeApiBaseUrl', 'PostCreate'), createModel), 'Failed to create new realtion');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.relationTypeResource#deleteById
     * @methodOf umbraco.resources.relationTypeResource
     *
     * @description
     * Deletes a relation type with a given ID.
     *
     * * ## Usage
     * <pre>
     * relationTypeResource.deleteById(1234).then(function() {
     *    alert('Deleted it!');
     * });
     * </pre>
     *
     * @param {Int} id The ID of the relation type to delete.
     * @returns {Promose} resourcePromise object.
     */
            deleteById: function deleteById(id) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('relationTypeApiBaseUrl', 'DeleteById', [{ id: id }])), 'Failed to delete item ' + id);
            },
            getPagedResults: function getPagedResults(id, options) {
                var defaults = {
                    pageSize: 25,
                    pageNumber: 1
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('relationTypeApiBaseUrl', 'GetPagedResults', {
                    id: id,
                    pageNumber: options.pageNumber,
                    pageSize: options.pageSize
                })), 'Failed to get paged relations for id ' + id);
            }
        };
    }
    angular.module('umbraco.resources').factory('relationTypeResource', relationTypeResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.sectionResource
    * @description Loads in data for section
    **/
    function sectionResource($q, $http, umbRequestHelper) {
        /** internal method to get the tree app url */
        function getSectionsUrl(section) {
            return Umbraco.Sys.ServerVariables.sectionApiBaseUrl + 'GetSections';
        }
        //the factory object returned
        return {
            /** Loads in the data to display the section list */
            getSections: function getSections() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('sectionApiBaseUrl', 'GetSections')), 'Failed to retrieve data for sections');
            },
            /** Loads in all available sections */
            getAllSections: function getAllSections() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('sectionApiBaseUrl', 'GetAllSections')), 'Failed to retrieve data for sections');
            }
        };
    }
    angular.module('umbraco.resources').factory('sectionResource', sectionResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.stylesheetResource
    * @description service to retrieve available stylesheets
    * 
    *
    **/
    function stylesheetResource($q, $http, umbRequestHelper) {
        //the factory object returned
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.stylesheetResource#getAll
     * @methodOf umbraco.resources.stylesheetResource
     *
     * @description
     * Gets all registered stylesheets
     *
     * ##usage
     * <pre>
     * stylesheetResource.getAll()
     *    .then(function(stylesheets) {
     *        alert('its here!');
     *    });
     * </pre> 
     * 
     * @returns {Promise} resourcePromise object containing the stylesheets.
     *
     */
            getAll: function getAll() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('stylesheetApiBaseUrl', 'GetAll')), 'Failed to retrieve stylesheets ');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.stylesheetResource#getRulesByName
     * @methodOf umbraco.resources.stylesheetResource
     *
     * @description
     * Returns all defined child rules for a stylesheet with a given name
     *
     * ##usage
     * <pre>
     * stylesheetResource.getRulesByName("ie7stylesheet")
     *    .then(function(rules) {
     *        alert('its here!');
     *    });
     * </pre> 
     * 
     * @returns {Promise} resourcePromise object containing the rules.
     *
     */
            getRulesByName: function getRulesByName(name) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('stylesheetApiBaseUrl', 'GetRulesByName', [{ name: name }])), 'Failed to retrieve stylesheets ');
            }
        };
    }
    angular.module('umbraco.resources').factory('stylesheetResource', stylesheetResource);
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.templateResource
    * @description Loads in data for templates
    **/
    function templateResource($q, $http, umbDataFormatter, umbRequestHelper, localizationService) {
        return {
            /**
     * @ngdoc method
     * @name umbraco.resources.templateResource#getById
     * @methodOf umbraco.resources.templateResource
     *
     * @description
     * Gets a template item with a given id
     *
     * ##usage
     * <pre>
     * templateResource.getById(1234)
     *    .then(function(template) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {Int} id id of template to retrieve
     * @returns {Promise} resourcePromise object.
     *
     */
            getById: function getById(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('templateApiBaseUrl', 'GetById', [{ id: id }])), 'Failed to retrieve data for template id ' + id);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.templateResource#getByAlias
     * @methodOf umbraco.resources.templateResource
     *
     * @description
     * Gets a template item with a given alias
     *
     * ##usage
     * <pre>
     * templateResource.getByAlias("upload")
     *    .then(function(template) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @param {String} alias Alias of template to retrieve
     * @returns {Promise} resourcePromise object.
     *
     */
            getByAlias: function getByAlias(alias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('templateApiBaseUrl', 'GetByAlias', [{ alias: alias }])), 'Failed to retrieve data for template with alias: ' + alias);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.templateResource#getAll
     * @methodOf umbraco.resources.templateResource
     *
     * @description
     * Gets all templates
     *
     * ##usage
     * <pre>
     * templateResource.getAll()
     *    .then(function(templates) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @returns {Promise} resourcePromise object.
     *
     */
            getAll: function getAll() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('templateApiBaseUrl', 'GetAll')), 'Failed to retrieve data');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.templateResource#getScaffold
     * @methodOf umbraco.resources.templateResource
     *
     * @description
     * Returns a scaffold of an empty template item
     *
     * The scaffold is used to build editors for templates that has not yet been populated with data.
     *
     * ##usage
     * <pre>
     * templateResource.getScaffold()
     *    .then(function(template) {
     *        alert('its here!');
     *    });
     * </pre>
     *
     * @returns {Promise} resourcePromise object containing the template scaffold.
     *
     */
            getScaffold: function getScaffold(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('templateApiBaseUrl', 'GetScaffold', [{ id: id }])), 'Failed to retrieve data for empty template');
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.templateResource#deleteById
     * @methodOf umbraco.resources.templateResource
     *
     * @description
     * Deletes a template with a given id
     *
     * ##usage
     * <pre>
     * templateResource.deleteById(1234)
     *    .then(function() {
     *        alert('its gone!');
     *    });
     * </pre>
     *
     * @param {Int} id id of template to delete
     * @returns {Promise} resourcePromise object.
     *
     */
            deleteById: function deleteById(id) {
                var promise = localizationService.localize('template_deleteByIdFailed', [id]);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('templateApiBaseUrl', 'DeleteById', [{ id: id }])), promise);
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.templateResource#save
     * @methodOf umbraco.resources.templateResource
     *
     * @description
     * Saves or update a template
     * 
     * ##usage
     * <pre>
     * templateResource.save(template)
     *    .then(function(template) {
     *        alert('its saved!');
     *    });
     * </pre>
     *
     * @param {Object} template object to save
     * @returns {Promise} resourcePromise object.
     *
     */
            save: function save(template) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('templateApiBaseUrl', 'PostSave'), template), 'Failed to save data for template id ' + template.id);
            }
        };
    }
    angular.module('umbraco.resources').factory('templateResource', templateResource);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.templateQueryResource
 * @function
 *
 * @description
 * Used by the query builder
 */
    (function () {
        'use strict';
        function templateQueryResource($http, umbRequestHelper) {
            /**
     * @ngdoc function
     * @name umbraco.resources.templateQueryResource#getAllowedProperties
     * @methodOf umbraco.resources.templateQueryResource
     * @function
     *
     * @description
     * Called to get allowed properties
     * ##usage
     * <pre>
     * templateQueryResource.getAllowedProperties()
     *    .then(function(response) {
     *
     *    });
     * </pre>
     */
            function getAllowedProperties() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('templateQueryApiBaseUrl', 'GetAllowedProperties')), 'Failed to retrieve properties');
            }
            /**
     * @ngdoc function
     * @name umbraco.resources.templateQueryResource#getContentTypes
     * @methodOf umbraco.resources.templateQueryResource
     * @function
     *
     * @description
     * Called to get content types
     * ##usage
     * <pre>
     * templateQueryResource.getContentTypes()
     *    .then(function(response) {
     *
     *    });
     * </pre>
     */
            function getContentTypes() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('templateQueryApiBaseUrl', 'GetContentTypes')), 'Failed to retrieve content types');
            }
            /**
     * @ngdoc function
     * @name umbraco.resources.templateQueryResource#getFilterConditions
     * @methodOf umbraco.resources.templateQueryResource
     * @function
     *
     * @description
     * Called to the filter conditions
     * ##usage
     * <pre>
     * templateQueryResource.getFilterConditions()
     *    .then(function(response) {
     *
     *    });
     * </pre>
     */
            function getFilterConditions() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('templateQueryApiBaseUrl', 'GetFilterConditions')), 'Failed to retrieve filter conditions');
            }
            /**
     * @ngdoc function
     * @name umbraco.resources.templateQueryResource#postTemplateQuery
     * @methodOf umbraco.resources.templateQueryResource
     * @function
     *
     * @description
     * Called to get content types
     * ##usage
     * <pre>
     * var query = {
     *     contentType: {
     *         name: "Everything"
     *      },
     *      source: {
     *          name: "My website"
     *      },
     *      filters: [
     *          {
     *              property: undefined,
     *              operator: undefined
     *          }
     *      ],
     *      sort: {
     *          property: {
     *              alias: "",
     *              name: "",
     *          },
     *          direction: "ascending"
     *      }
     *  };
     * 
     * templateQueryResource.postTemplateQuery(query)
     *    .then(function(response) {
     *
     *    });
     * </pre>
     * @param {object} query Query to build result
     */
            function postTemplateQuery(query) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('templateQueryApiBaseUrl', 'PostTemplateQuery'), query), 'Failed to retrieve query');
            }
            var resource = {
                getAllowedProperties: getAllowedProperties,
                getContentTypes: getContentTypes,
                getFilterConditions: getFilterConditions,
                postTemplateQuery: postTemplateQuery
            };
            return resource;
        }
        angular.module('umbraco.resources').factory('templateQueryResource', templateQueryResource);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.usersResource
 * @function
 *
 * @description
 * Used by the users section to get users and send requests to create, invite, delete, etc. users.
 */
    (function () {
        'use strict';
        function tourResource($http, umbRequestHelper, $q, umbDataFormatter) {
            function getTours() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('tourApiBaseUrl', 'GetTours')), 'Failed to get tours');
            }
            function getToursForDoctype(doctypeAlias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('tourApiBaseUrl', 'GetToursForDoctype', [{ doctypeAlias: doctypeAlias }])), 'Failed to get tours');
            }
            var resource = {
                getTours: getTours,
                getToursForDoctype: getToursForDoctype
            };
            return resource;
        }
        angular.module('umbraco.resources').factory('tourResource', tourResource);
    }());
    'use strict';
    /**
    * @ngdoc service
    * @name umbraco.resources.treeResource
    * @description Loads in data for trees
    **/
    function treeResource($q, $http, umbRequestHelper) {
        /** internal method to get the tree node's children url */
        function getTreeNodesUrl(node) {
            if (!node.childNodesUrl) {
                throw 'No childNodesUrl property found on the tree node, cannot load child nodes';
            }
            return node.childNodesUrl;
        }
        /** internal method to get the tree menu url */
        function getTreeMenuUrl(node) {
            if (!node.menuUrl) {
                return null;
            }
            return node.menuUrl;
        }
        //the factory object returned
        return {
            /** Loads in the data to display the nodes menu */
            loadMenu: function loadMenu(node) {
                var treeMenuUrl = getTreeMenuUrl(node);
                if (treeMenuUrl !== undefined && treeMenuUrl !== null && treeMenuUrl.length > 0) {
                    return umbRequestHelper.resourcePromise($http.get(getTreeMenuUrl(node)), 'Failed to retrieve data for a node\'s menu ' + node.id);
                } else {
                    return $q.reject({ errorMsg: 'No tree menu url defined for node ' + node.id });
                }
            },
            /** Loads in the data to display the nodes for an application */
            loadApplication: function loadApplication(options) {
                if (!options || !options.section) {
                    throw 'The object specified for does not contain a \'section\' property';
                }
                if (!options.tree) {
                    options.tree = '';
                }
                if (!options.isDialog) {
                    options.isDialog = false;
                }
                //create the query string for the tree request, these are the mandatory options:
                var query = 'application=' + options.section + '&tree=' + options.tree + '&use=' + (options.isDialog ? 'dialog' : 'main');
                //the options can contain extra query string parameters
                if (options.queryString) {
                    query += '&' + options.queryString;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('treeApplicationApiBaseUrl', 'GetApplicationTrees', query)), 'Failed to retrieve data for application tree ' + options.section);
            },
            /** Loads in the data to display the child nodes for a given node */
            loadNodes: function loadNodes(options) {
                if (!options || !options.node) {
                    throw 'The options parameter object does not contain the required properties: \'node\'';
                }
                return umbRequestHelper.resourcePromise($http.get(getTreeNodesUrl(options.node)), 'Failed to retrieve data for child nodes ' + options.node.nodeId);
            }
        };
    }
    angular.module('umbraco.resources').factory('treeResource', treeResource);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.usersResource
 * @function
 *
 * @description
 * Used by the users section to get users and send requests to create, invite, delete, etc. users.
 */
    (function () {
        'use strict';
        function userGroupsResource($http, umbRequestHelper, $q, umbDataFormatter) {
            function getUserGroupScaffold() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('userGroupsApiBaseUrl', 'GetEmptyUserGroup')), 'Failed to get the user group scaffold');
            }
            function saveUserGroup(userGroup, isNew) {
                if (!userGroup) {
                    throw 'userGroup not specified';
                }
                //need to convert the user data into the correctly formatted save data - it is *not* the same and we don't want to over-post
                var formattedSaveData = umbDataFormatter.formatUserGroupPostData(userGroup, 'save' + (isNew ? 'New' : ''));
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userGroupsApiBaseUrl', 'PostSaveUserGroup'), formattedSaveData), 'Failed to save user group');
            }
            function getUserGroup(id) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('userGroupsApiBaseUrl', 'GetUserGroup', { id: id })), 'Failed to retrieve data for user group ' + id);
            }
            function getUserGroups(args) {
                if (!args) {
                    args = { onlyCurrentUserGroups: true };
                }
                if (args.onlyCurrentUserGroups === undefined || args.onlyCurrentUserGroups === null) {
                    args.onlyCurrentUserGroups = true;
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('userGroupsApiBaseUrl', 'GetUserGroups', args)), 'Failed to retrieve user groups');
            }
            function deleteUserGroups(userGroupIds) {
                var query = 'userGroupIds=' + userGroupIds.join('&userGroupIds=');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userGroupsApiBaseUrl', 'PostDeleteUserGroups', query)), 'Failed to delete user groups');
            }
            var resource = {
                saveUserGroup: saveUserGroup,
                getUserGroup: getUserGroup,
                getUserGroups: getUserGroups,
                getUserGroupScaffold: getUserGroupScaffold,
                deleteUserGroups: deleteUserGroups
            };
            return resource;
        }
        angular.module('umbraco.resources').factory('userGroupsResource', userGroupsResource);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.resources.usersResource
 * @function
 *
 * @description
 * Used by the users section to get users and send requests to create, invite, disable, etc. users.
 */
    (function () {
        'use strict';
        function usersResource($http, umbRequestHelper, $q, umbDataFormatter) {
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#clearAvatar
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Deletes the user avatar
      *
      * ##usage
      * <pre>
      * usersResource.clearAvatar(1)
      *    .then(function() {
      *        alert("avatar is gone");
      *    });
      * </pre>
      * 
      * @param {Array} id id of user.
      * @returns {Promise} resourcePromise object.
      *
      */
            function clearAvatar(userId) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostClearAvatar', { id: userId })), 'Failed to clear the user avatar ' + userId);
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#disableUsers
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Disables a collection of users
      *
      * ##usage
      * <pre>
      * usersResource.disableUsers([1, 2, 3, 4, 5])
      *    .then(function() {
      *        alert("users were disabled");
      *    });
      * </pre>
      * 
      * @param {Array} ids ids of users to disable.
      * @returns {Promise} resourcePromise object.
      *
      */
            function disableUsers(userIds) {
                if (!userIds) {
                    throw 'userIds not specified';
                }
                //we need to create a custom query string for the usergroup array, so create it now and we can append the user groups if needed
                var qry = 'userIds=' + userIds.join('&userIds=');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostDisableUsers', qry)), 'Failed to disable the users ' + userIds.join(','));
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#enableUsers
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Enables a collection of users
      *
      * ##usage
      * <pre>
      * usersResource.enableUsers([1, 2, 3, 4, 5])
      *    .then(function() {
      *        alert("users were enabled");
      *    });
      * </pre>
      * 
      * @param {Array} ids ids of users to enable.
      * @returns {Promise} resourcePromise object.
      *
      */
            function enableUsers(userIds) {
                if (!userIds) {
                    throw 'userIds not specified';
                }
                //we need to create a custom query string for the usergroup array, so create it now and we can append the user groups if needed
                var qry = 'userIds=' + userIds.join('&userIds=');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostEnableUsers', qry)), 'Failed to enable the users ' + userIds.join(','));
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#unlockUsers
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Unlocks a collection of users
      *
      * ##usage
      * <pre>
      * usersResource.unlockUsers([1, 2, 3, 4, 5])
      *    .then(function() {
      *        alert("users were unlocked");
      *    });
      * </pre>
      * 
      * @param {Array} ids ids of users to unlock.
      * @returns {Promise} resourcePromise object.
      *
      */
            function unlockUsers(userIds) {
                if (!userIds) {
                    throw 'userIds not specified';
                }
                //we need to create a custom query string for the usergroup array, so create it now and we can append the user groups if needed
                var qry = 'userIds=' + userIds.join('&userIds=');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostUnlockUsers', qry)), 'Failed to enable the users ' + userIds.join(','));
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#setUserGroupsOnUsers
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Overwrites the existing user groups on a collection of users
      *
      * ##usage
      * <pre>
      * usersResource.setUserGroupsOnUsers(['admin', 'editor'], [1, 2, 3, 4, 5])
      *    .then(function() {
      *        alert("users were updated");
      *    });
      * </pre>
      * 
      * @param {Array} userGroupAliases aliases of user groups.
      * @param {Array} ids ids of users to update.
      * @returns {Promise} resourcePromise object.
      *
      */
            function setUserGroupsOnUsers(userGroups, userIds) {
                var userGroupAliases = userGroups.map(function (o) {
                    return o.alias;
                });
                var query = 'userGroupAliases=' + userGroupAliases.join('&userGroupAliases=') + '&userIds=' + userIds.join('&userIds=');
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostSetUserGroupsOnUsers', query)), 'Failed to set user groups ' + userGroupAliases.join(',') + ' on the users ' + userIds.join(','));
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#getPagedResults
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Get users
      *
      * ##usage
      * <pre>
      * usersResource.getPagedResults({pageSize: 10, pageNumber: 2})
      *    .then(function(data) {
      *        var users = data.items;
      *        alert('they are here!');
      *    });
      * </pre>
      * 
      * @param {Object} options optional options object
      * @param {Int} options.pageSize if paging data, number of users per page, default = 25
      * @param {Int} options.pageNumber if paging data, current page index, default = 1
      * @param {String} options.filter if provided, query will only return those with names matching the filter
      * @param {String} options.orderDirection can be `Ascending` or `Descending` - Default: `Ascending`
      * @param {String} options.orderBy property to order users by, default: `Username`
      * @param {Array} options.userGroups property to filter users by user group
      * @param {Array} options.userStates property to filter users by user state
      * @returns {Promise} resourcePromise object containing an array of content items.
      *
      */
            function getPagedResults(options) {
                var defaults = {
                    pageSize: 25,
                    pageNumber: 1,
                    filter: '',
                    orderDirection: 'Ascending',
                    orderBy: 'Username',
                    userGroups: [],
                    userStates: []
                };
                if (options === undefined) {
                    options = {};
                }
                //overwrite the defaults if there are any specified
                angular.extend(defaults, options);
                //now copy back to the options we will use
                options = defaults;
                //change asc/desct
                if (options.orderDirection === 'asc') {
                    options.orderDirection = 'Ascending';
                } else if (options.orderDirection === 'desc') {
                    options.orderDirection = 'Descending';
                }
                var params = {
                    pageNumber: options.pageNumber,
                    pageSize: options.pageSize,
                    orderBy: options.orderBy,
                    orderDirection: options.orderDirection,
                    filter: options.filter
                };
                //we need to create a custom query string for the usergroup array, so create it now and we can append the user groups if needed
                var qry = umbRequestHelper.dictionaryToQueryString(params);
                if (options.userGroups.length > 0) {
                    //we need to create a custom query string for an array
                    qry += '&userGroups=' + options.userGroups.join('&userGroups=');
                }
                if (options.userStates.length > 0) {
                    //we need to create a custom query string for an array
                    qry += '&userStates=' + options.userStates.join('&userStates=');
                }
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('userApiBaseUrl', 'GetPagedUsers', qry)), 'Failed to retrieve users paged result');
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#getUser
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Gets a user
      *
      * ##usage
      * <pre>
      * usersResource.getUser(1)
      *    .then(function(user) {
      *        alert("It's here");
      *    });
      * </pre>
      * 
      * @param {Int} userId user id.
      * @returns {Promise} resourcePromise object containing the user.
      *
      */
            function getUser(userId) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('userApiBaseUrl', 'GetById', { id: userId })), 'Failed to retrieve data for user ' + userId);
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#getUsers
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Gets users from ids
      *
      * ##usage
      * <pre>
      * usersResource.getUsers([1,2,3])
      *    .then(function(data) {
      *        alert("It's here");
      *    });
      * </pre>
      * 
      * @param {Array} userIds user ids.
      * @returns {Promise} resourcePromise object containing the users array.
      *
      */
            function getUsers(userIds) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('userApiBaseUrl', 'GetByIds', { ids: userIds })), 'Failed to retrieve data for users ' + userIds);
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#createUser
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Creates a new user
      *
      * ##usage
      * <pre>
      * usersResource.createUser(user)
      *    .then(function(newUser) {
      *        alert("It's here");
      *    });
      * </pre>
      * 
      * @param {Object} user user to create
      * @returns {Promise} resourcePromise object containing the new user.
      *
      */
            function createUser(user) {
                if (!user) {
                    throw 'user not specified';
                }
                //need to convert the user data into the correctly formatted save data - it is *not* the same and we don't want to over-post
                var formattedSaveData = umbDataFormatter.formatUserPostData(user);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostCreateUser'), formattedSaveData), 'Failed to save user');
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#inviteUser
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Creates and sends an email invitation to a new user
      *
      * ##usage
      * <pre>
      * usersResource.inviteUser(user)
      *    .then(function(newUser) {
      *        alert("It's here");
      *    });
      * </pre>
      * 
      * @param {Object} user user to invite
      * @returns {Promise} resourcePromise object containing the new user.
      *
      */
            function inviteUser(user) {
                if (!user) {
                    throw 'user not specified';
                }
                //need to convert the user data into the correctly formatted save data - it is *not* the same and we don't want to over-post
                var formattedSaveData = umbDataFormatter.formatUserPostData(user);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostInviteUser'), formattedSaveData), 'Failed to invite user');
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#saveUser
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Saves a user
      *
      * ##usage
      * <pre>
      * usersResource.saveUser(user)
      *    .then(function(updatedUser) {
      *        alert("It's here");
      *    });
      * </pre>
      * 
      * @param {Object} user object to save
      * @returns {Promise} resourcePromise object containing the updated user.
      *
      */
            function saveUser(user) {
                if (!user) {
                    throw 'user not specified';
                }
                //need to convert the user data into the correctly formatted save data - it is *not* the same and we don't want to over-post
                var formattedSaveData = umbDataFormatter.formatUserPostData(user);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostSaveUser'), formattedSaveData), 'Failed to save user');
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#changePassword
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Changes a user's password
      *
      * ##usage
      * <pre>
      * usersResource.changePassword(changePasswordModel)
      *    .then(function() {
      *        // password changed
      *    });
      * </pre>
      * 
      * @param {Object} model object to save
      * @returns {Promise} resourcePromise object containing the updated user.
      *
      */
            function changePassword(changePasswordModel) {
                if (!changePasswordModel) {
                    throw 'password model not specified';
                }
                //need to convert the password data into the correctly formatted save data - it is *not* the same and we don't want to over-post
                var formattedPasswordData = umbDataFormatter.formatChangePasswordModel(changePasswordModel);
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostChangePassword'), formattedPasswordData), 'Failed to save user');
            }
            /**
      * @ngdoc method
      * @name umbraco.resources.usersResource#deleteNonLoggedInUser
      * @methodOf umbraco.resources.usersResource
      *
      * @description
      * Deletes a user that hasn't already logged in (and hence we know has made no content updates that would create related records)
      *
      * ##usage
      * <pre>
      * usersResource.deleteNonLoggedInUser(1)
      *    .then(function() {
      *        alert("user was deleted");
      *    });
      * </pre>
      * 
      * @param {Int} userId user id.
      * @returns {Promise} resourcePromise object.
      *
      */
            function deleteNonLoggedInUser(userId) {
                return umbRequestHelper.resourcePromise($http.post(umbRequestHelper.getApiUrl('userApiBaseUrl', 'PostDeleteNonLoggedInUser', { id: userId })), 'Failed to delete the user ' + userId);
            }
            var resource = {
                disableUsers: disableUsers,
                enableUsers: enableUsers,
                unlockUsers: unlockUsers,
                setUserGroupsOnUsers: setUserGroupsOnUsers,
                getPagedResults: getPagedResults,
                getUser: getUser,
                getUsers: getUsers,
                createUser: createUser,
                inviteUser: inviteUser,
                saveUser: saveUser,
                changePassword: changePassword,
                deleteNonLoggedInUser: deleteNonLoggedInUser,
                clearAvatar: clearAvatar
            };
            return resource;
        }
        angular.module('umbraco.resources').factory('usersResource', usersResource);
    }());
}());