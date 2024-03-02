import type { UmbMemberTypeDetailModel } from '../types.js';
import { UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT } from '../repository/detail/member-type-detail.store.js';
import type { UmbMemberTypeTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMemberTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for MemberType Items
 */
export class UmbMemberTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMemberTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMemberTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbMemberTypeTreeItemModel, UmbMemberTypeDetailModel>(
			host,
			this,
			UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT,
			(item) => this.#createTreeItemMapper(item),
			(item) => this.#updateTreeItemMapper(item),
		);
	}

	// TODO: revisit this when we have decided on detail model sizes
	#createTreeItemMapper = (item: UmbMemberTypeDetailModel) => {
		const treeItem: UmbMemberTypeTreeItemModel = {
			unique: item.unique,
			parentUnique: null,
			name: item.name,
			entityType: item.entityType,
			isFolder: false,
			hasChildren: false,
		};

		return treeItem;
	};

	// TODO: revisit this when we have decided on detail model sizes
	#updateTreeItemMapper = (item: UmbMemberTypeDetailModel) => {
		return {
			name: item.name,
			icon: item.icon,
		};
	};
}

export default UmbMemberTypeTreeStore;

export const UMB_MEMBER_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbMemberTypeTreeStore>('UmbMemberTypeTreeStore');
