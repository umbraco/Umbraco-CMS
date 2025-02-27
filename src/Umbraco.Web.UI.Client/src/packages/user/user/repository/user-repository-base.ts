import type { UmbUserDetailStore } from './detail/user-detail.store.js';
import { UMB_USER_DETAIL_STORE_CONTEXT } from './detail/user-detail.store.token.js';
import type { UmbUserItemStore } from './item/user-item.store.js';
import { UMB_USER_ITEM_STORE_CONTEXT } from './item/user-item.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export abstract class UmbUserRepositoryBase extends UmbRepositoryBase {
	protected init;
	protected detailStore?: UmbUserDetailStore;
	protected itemStore?: UmbUserItemStore;
	protected notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.init = Promise.all([
			this.consumeContext(UMB_USER_DETAIL_STORE_CONTEXT, (instance) => {
				this.detailStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_USER_ITEM_STORE_CONTEXT, (instance) => {
				this.itemStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.notificationContext = instance;
			}).asPromise(),
		]);
	}
}
