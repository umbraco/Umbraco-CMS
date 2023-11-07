import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from './user.store.js';
import { UMB_USER_ITEM_STORE_CONTEXT_TOKEN, UmbUserItemStore } from './item/user-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbUserRepositoryBase extends UmbRepositoryBase {
	protected init;

	protected detailStore?: UmbUserStore;
	protected itemStore?: UmbUserItemStore;
	protected notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.init = Promise.all([
			this.consumeContext(UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.detailStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_USER_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.itemStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.notificationContext = instance;
			}).asPromise()
		]);
	}
}
