import type { UmbMediaTypeTreeItemModel } from '../types.js';
import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
	UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import { UmbManagementApiMediaTypeTreeDataRequestManager } from './media-type-tree.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { MediaTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeDataSource,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Media Type tree that fetches data from the server
 * @class UmbMediaTypeTreeServerDataSource
 */
export class UmbMediaTypeTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbMediaTypeTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiMediaTypeTreeDataRequestManager(this);

	async getRootItems(args: UmbTreeRootItemsRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getRootItems(args);

		const mappedData = data
			? {
					...data,
					items: data?.items.map((item) => this.#mapItem(item)),
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getChildrenOf(args: UmbTreeChildrenOfRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getChildrenOf(args);

		const mappedData = data
			? {
					...data,
					items: data?.items.map((item) => this.#mapItem(item)),
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getAncestorsOf(args: UmbTreeAncestorsOfRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getAncestorsOf(args);

		const mappedData = data?.map((item) => this.#mapItem(item));

		return { data: mappedData, error };
	}

	#mapItem(item: MediaTypeTreeItemResponseModel): UmbMediaTypeTreeItemModel {
		return {
			unique: item.id,
			parent: {
				unique: item.parent ? item.parent.id : null,
				entityType: item.parent ? UMB_MEDIA_TYPE_ENTITY_TYPE : UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
			},
			name: item.name,
			entityType: item.isFolder ? UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE : UMB_MEDIA_TYPE_ENTITY_TYPE,
			hasChildren: item.hasChildren,
			isFolder: item.isFolder,
			icon: item.icon,
		};
	}
}
