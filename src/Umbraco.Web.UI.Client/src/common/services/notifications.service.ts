/// <reference path="../definitions/global.d.ts" />
/// <reference path="angularhelper.service.ts" />
/// <reference path="../definitions/string.ts" />

namespace umbraco.services {

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
	export class notificationsService {
		private nArray: Array<any> = new Array<any>();

		public current: Array<any> = this.nArray;

		private $rootScope: any;
		private $timeout: any;
		private angularHelper: umbraco.services.angularHelper;

		public constructor($rootScope, $timeout, angularHelper: umbraco.services.angularHelper) {
			this.$rootScope = $rootScope;
			this.$timeout = $timeout;
			this.angularHelper = angularHelper;
		}

        public add(item: models.iNotification) {
            var that = this;
			this.angularHelper.safeApply(this.$rootScope, function () {

				if(item.view){
					item.view = this.setViewPath(item.view);
					item.sticky = true;
					item.type = models.notificationType.form;
					item.headline = null;
				}


				//add a colon after the headline if there is a message as well
				if (item.message) {
					item.headline += ": ";
					if(item.message.length > 200) {
						item.sticky = true;
					}
				}	
			
				//we need to ID the item, going by index isn't good enough because people can remove at different indexes 
				// whenever they want. Plus once we remove one, then the next index will be different. The only way to 
				// effectively remove an item is by an Id.
				item.id = String.CreateGuid();

                that.nArray.push(item);

				if(!item.sticky) {
					this.$timeout(
                        function() {
    						var found = _.find(this.nArray, function(i) {
        						return i.id === item.id;
        					});
        					if (found) {
        						var index = this.nArray.indexOf(found);
        						this.nArray.splice(index, 1);
        					}
    					}
                    , 10000);
				}

				return item;
			});
		}

		public hasView(view){
			if(!view){
				return _.find(this.nArray, function(notification){ return notification.view;});
			}else{
				view = this.setViewPath(view).toLowerCase();
				return _.find(this.nArray, function(notification){ return notification.view.toLowerCase() === view;});
			}	
		}

		public addView(view: string, args: Array<any>){
			var item = {
				args: args,
				view: view
			};

			this.add(item);
		}

		public showNotification(notification: umbraco.services.models.iGenericNotification) {
            if (!notification) {
                throw "notification cannot be null";
            }
            if (notification.type === undefined || notification.type === null) {
                throw "notification.type cannot be null";
            }
            if (!notification.header) {
                throw "notification.header cannot be null";
            }
            
            switch(notification.type) {
                case models.genericNotificationType.save:
                    this.success(notification.header, notification.message);
                    break;
                case models.genericNotificationType.info:
                    this.success(notification.header, notification.message);
                    break;
                case models.genericNotificationType.error:
                    this.error(notification.header, notification.message);
                    break;
                case models.genericNotificationType.success:
                    this.success(notification.header, notification.message);
                    break;
                case models.genericNotificationType.warning:
                    this.warning(notification.header, notification.message);
                    break;
            }
        }

		public success(headline: string, message: string) {
	        return this.add({ headline: headline, message: message, type: models.notificationType.success, time: new Date() });
	    }

		public error(headline: string, message: string) {
	        return this.add({ headline: headline, message: message, type: models.notificationType.error, time: new Date() });
		}

		public  warning(headline: string, message: string) {
	        return this.add({ headline: headline, message: message, type: models.notificationType.warning, time: new Date() });
		}

		public info(headline: string, message: string) {
	        return this.add({ headline: headline, message: message, type: models.notificationType.info, time: new Date() });
		}

		public remove(index) {
			if(angular.isObject(index)){
				var i = this.nArray.indexOf(index);
				this.angularHelper.safeApply(this.$rootScope, function() {
				    this.nArray.splice(i, 1);
				});
			}else{
				this.angularHelper.safeApply(this.$rootScope, function() {
				    this.nArray.splice(index, 1);
				});	
			}
		}

		public removeAll() {
	        this.angularHelper.safeApply(this.$rootScope, function() {
	            this.nArray = [];
	        });
		}

		public getCurrent(){
			return this.nArray;
		}

		private setViewPath(view: string) : string {
			if(view.indexOf('/') < 0)
			{
				view = "views/common/notifications/" + view;
			}

			if(view.indexOf('.html') < 0)
			{
				view = view + ".html";
			}
			return view;
		}
	}

	/*
		Models for Notification Service
	*/
	export namespace models {

		export interface iNotification {
			id?: string;
			headline?: string;
			message?: string;
			type?: notificationType;
			url?: string;
			view?: string;
			actions?: Array<any>;
			sticky?: boolean;
			time?: Date;
			args?: Array<any>;
		}

		export enum notificationType {
			info = 'info',
			error = 'error',
			success = 'success',
			warning = 'warning',
			form = 'form'
		}

		export interface iGenericNotification {
			header?: string;
			message?: string;
			type?: genericNotificationType;
			url?: string;
			view?: string;
			actions?: Array<any>;
			sticky?: boolean;
			time?: Date;
		}

		export enum genericNotificationType {
			save = 0,
			info = 1,
			error = 2,
			success = 3,
			warning = 4
		}

		
	}
	
}

angular.module('umbraco.services').service('notificationsService', ['$rootScope', '$timeout', 'angularHelper', umbraco.services.notificationsService]);

