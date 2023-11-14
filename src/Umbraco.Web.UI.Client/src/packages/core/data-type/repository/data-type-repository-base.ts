import { UMB_DATA_TYPE_TREE_STORE_CONTEXT, UmbDataTypeTreeStore } from '../tree/data-type.tree.store.js';
import { UMB_DATA_TYPE_ITEM_STORE_CONTEXT, UmbDataTypeItemStore } from './item/data-type-item.store.js';
import { UMB_DATA_TYPE_STORE_CONTEXT, UmbDataTypeDetailStore } from './detail/data-type-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDataTypeRepositoryBase extends UmbRepositoryBase {
	protected _init: Promise<unknown>;

	protected _detailStore?: UmbDataTypeDetailStore;
	protected _treeStore?: UmbDataTypeTreeStore;
	protected _itemStore?: UmbDataTypeItemStore;

	protected _notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this._init = Promise.all([
			this.consumeContext(UMB_DATA_TYPE_STORE_CONTEXT, (instance) => {
				this._detailStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_DATA_TYPE_TREE_STORE_CONTEXT, (instance) => {
				this._treeStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_DATA_TYPE_ITEM_STORE_CONTEXT, (instance) => {
				this._itemStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this._notificationContext = instance;
			}).asPromise(),
		]);
	}
}
