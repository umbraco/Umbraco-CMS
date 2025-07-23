import { UMB_MEMBER_ITEM_STORE_CONTEXT } from '../item/repository/member-item.store.context-token.js';
import { UMB_MEMBER_DETAIL_STORE_CONTEXT } from './detail/member-detail.store.context-token.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbMemberRepositoryBase extends UmbRepositoryBase {
	protected init;
	protected detailStore?: typeof UMB_MEMBER_DETAIL_STORE_CONTEXT.TYPE;
	protected itemStore?: typeof UMB_MEMBER_ITEM_STORE_CONTEXT.TYPE;
	protected notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.init = Promise.all([
			this.consumeContext(UMB_MEMBER_DETAIL_STORE_CONTEXT, (instance) => {
				this.detailStore = instance;
			})
				.asPromise({ preventTimeout: true })
				// Ignore the error, we can assume that the flow was stopped (asPromise failed), but it does not mean that the consumption was not successful.
				.catch(() => undefined),

			this.consumeContext(UMB_MEMBER_ITEM_STORE_CONTEXT, (instance) => {
				this.itemStore = instance;
			})
				.asPromise({ preventTimeout: true })
				.catch(() => undefined),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.notificationContext = instance;
			})
				.asPromise({ preventTimeout: true })
				.catch(() => undefined),
		]);
	}
}
