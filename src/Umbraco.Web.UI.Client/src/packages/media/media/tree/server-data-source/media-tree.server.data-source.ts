import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import type {
	UmbMediaTreeChildrenOfRequestArgs,
	UmbMediaTreeItemModel,
	UmbMediaTreeRootItemsRequestArgs,
} from '../types.js';
import { UmbManagementApiMediaTreeDataRequestManager } from './media-tree.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { MediaTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbTreeAncestorsOfRequestArgs, UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Media tree that fetches data from the server
 * @class UmbMediaTreeServerDataSource
 */
export class UmbMediaTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbMediaTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiMediaTreeDataRequestManager(this);

	async getRootItems(args: UmbMediaTreeRootItemsRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getRootItems(args);

		const mappedData = data
			? {
					...data,
					items: data?.items.map((item) => this.#mapItem(item)),
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getChildrenOf(args: UmbMediaTreeChildrenOfRequestArgs) {
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

	#mapItem(item: MediaTreeItemResponseModel): UmbMediaTreeItemModel {
		return {
			unique: item.id,
			parent: {
				unique: item.parent ? item.parent.id : null,
				entityType: item.parent ? UMB_MEDIA_ENTITY_TYPE : UMB_MEDIA_ROOT_ENTITY_TYPE,
			},
			entityType: UMB_MEDIA_ENTITY_TYPE,
			hasChildren: item.hasChildren,
			noAccess: item.noAccess,
			isTrashed: item.isTrashed,
			isFolder: false,
			mediaType: {
				unique: item.mediaType.id,
				icon: item.mediaType.icon,
				collection: item.mediaType.collection ? { unique: item.mediaType.collection.id } : null,
			},
			name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
			variants: item.variants.map((variant) => {
				return {
					name: variant.name,
					culture: variant.culture || null,
				};
			}),
			createDate: item.createDate,
			flags: item.flags,
		};
	}
}
