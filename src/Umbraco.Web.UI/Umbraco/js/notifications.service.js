/// <reference path="../definitions/global.d.ts" />
/// <reference path="angularhelper.service.ts" />
/// <reference path="../definitions/string.ts" />
var umbraco;
(function (umbraco) {
    var services;
    (function (services) {
        /**
         * @ngdoc service
         * @name umbraco.services.notificationsService
         *
         * @requires $rootScope
         * @requires $timeout
         * @requires angularHelper
         *
        * @description
        * Application-wide service for handling notifications, the umbraco application
        * maintains a single collection of notications, which the UI watches for changes.
        * By default when a notication is added, it is automaticly removed 7 seconds after
        * This can be changed on add()
        *
        * ##usage
        * To use, simply inject the notificationsService into any controller that needs it, and make
        * sure the umbraco.services module is accesible - which it should be by default.
        *
        * <pre>
        *		notificationsService.success("Document Published", "hooraaaay for you!");
        *      notificationsService.error("Document Failed", "booooh");
        * </pre>
        */
        class notificationsService {
            constructor($rootScope, $timeout, angularHelper) {
                this.nArray = new Array();
                this.current = this.nArray;
                this.$rootScope = $rootScope;
                this.$timeout = $timeout;
                this.angularHelper = angularHelper;
            }
            add(item) {
                this.angularHelper.safeApply(this.$rootScope, function () {
                    if (item.view) {
                        item.view = this.setViewPath(item.view);
                        item.sticky = true;
                        item.type = "form";
                        item.headline = null;
                    }
                    //add a colon after the headline if there is a message as well
                    if (item.message) {
                        item.headline += ": ";
                        if (item.message.length > 200) {
                            item.sticky = true;
                        }
                    }
                    //we need to ID the item, going by index isn't good enough because people can remove at different indexes 
                    // whenever they want. Plus once we remove one, then the next index will be different. The only way to 
                    // effectively remove an item is by an Id.
                    item.id = String.CreateGuid();
                    this.nArray.push(item);
                    if (!item.sticky) {
                        this.$timeout(function () {
                            var found = _.find(this.nArray, function (i) {
                                return i.id === item.id;
                            });
                            if (found) {
                                var index = this.nArray.indexOf(found);
                                this.nArray.splice(index, 1);
                            }
                        }, 10000);
                    }
                    return item;
                });
            }
            hasView(view) {
                if (!view) {
                    return _.find(this.nArray, function (notification) { return notification.view; });
                }
                else {
                    view = this.setViewPath(view).toLowerCase();
                    return _.find(this.nArray, function (notification) { return notification.view.toLowerCase() === view; });
                }
            }
            addView(view, args) {
                var item = {
                    args: args,
                    view: view
                };
                this.add(item);
            }
            showNotification(args) {
                if (!args) {
                    throw "args cannot be null";
                }
                if (args.type === undefined || args.type === null) {
                    throw "args.type cannot be null";
                }
                if (!args.header) {
                    throw "args.header cannot be null";
                }
                switch (args.type) {
                    case 0:
                        //save
                        this.success(args.header, args.message);
                        break;
                    case 1:
                        //info
                        this.success(args.header, args.message);
                        break;
                    case 2:
                        //error
                        this.error(args.header, args.message);
                        break;
                    case 3:
                        //success
                        this.success(args.header, args.message);
                        break;
                    case 4:
                        //warning
                        this.warning(args.header, args.message);
                        break;
                }
            }
            success(headline, message) {
                return this.add({ headline: headline, message: message, type: 'success', time: new Date() });
            }
            error(headline, message) {
                return this.add({ headline: headline, message: message, type: 'error', time: new Date() });
            }
            warning(headline, message) {
                return this.add({ headline: headline, message: message, type: 'warning', time: new Date() });
            }
            info(headline, message) {
                return this.add({ headline: headline, message: message, type: 'info', time: new Date() });
            }
            remove(index) {
                if (angular.isObject(index)) {
                    var i = this.nArray.indexOf(index);
                    this.angularHelper.safeApply(this.$rootScope, function () {
                        this.nArray.splice(i, 1);
                    });
                }
                else {
                    this.angularHelper.safeApply(this.$rootScope, function () {
                        this.nArray.splice(index, 1);
                    });
                }
            }
            removeAll() {
                this.angularHelper.safeApply(this.$rootScope, function () {
                    this.nArray = [];
                });
            }
            getCurrent() {
                return this.nArray;
            }
            setViewPath(view) {
                if (view.indexOf('/') < 0) {
                    view = "views/common/notifications/" + view;
                }
                if (view.indexOf('.html') < 0) {
                    view = view + ".html";
                }
                return view;
            }
        }
        services.notificationsService = notificationsService;
    })(services = umbraco.services || (umbraco.services = {}));
})(umbraco || (umbraco = {}));
angular.module('umbraco.services').service('notificationsService', ['$rootScope', '$timeout', 'angularHelper', umbraco.services.notificationsService]);
