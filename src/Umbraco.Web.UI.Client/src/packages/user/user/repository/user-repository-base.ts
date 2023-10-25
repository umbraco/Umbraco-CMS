import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from './user.store.js';
import { UMB_USER_ITEM_STORE_CONTEXT_TOKEN, UmbUserItemStore } from './item/user-item.store.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

export class UmbUserRepositoryBase {
	protected host;
	protected init;

	protected detailStore?: UmbUserStore;
	protected itemStore?: UmbUserItemStore;
	protected notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.host = host;

		this.init = Promise.all([
			new UmbContextConsumerController(this.host, UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.detailStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.host, UMB_USER_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.itemStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.notificationContext = instance;
			}).asPromise(),
		]);
	}
}
