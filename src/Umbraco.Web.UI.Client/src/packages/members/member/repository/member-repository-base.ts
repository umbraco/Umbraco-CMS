import type { UmbMemberItemStore } from '../item/repository/member-item.store.js';
import { UMB_MEMBER_ITEM_STORE_CONTEXT } from '../item/repository/member-item.store.context-token.js';
import type { UmbMemberDetailStore } from './detail/member-detail.store.js';
import { UMB_MEMBER_DETAIL_STORE_CONTEXT } from './detail/member-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export abstract class UmbMemberRepositoryBase extends UmbRepositoryBase {
	protected init;
	protected detailStore?: UmbMemberDetailStore;
	protected itemStore?: UmbMemberItemStore;
	protected notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.init = Promise.all([
			this.consumeContext(UMB_MEMBER_DETAIL_STORE_CONTEXT, (instance) => {
				if (instance) {
					this.detailStore = instance;
				}
			}).asPromise({ preventTimeout: true }),

			this.consumeContext(UMB_MEMBER_ITEM_STORE_CONTEXT, (instance) => {
				if (instance) {
					this.itemStore = instance;
				}
			}).asPromise({ preventTimeout: true }),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				if (instance) {
					this.notificationContext = instance;
				}
			}).asPromise({ preventTimeout: true }),
		]);
	}
}
