import { UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbElementTreeItemModel } from '../types.js';
import { UmbManagementApiElementTreeDataRequestManager } from './element-tree.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { ElementTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeDataSource,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Element tree that fetches data from the server
 * @class UmbElementTreeServerDataSource
 */
export class UmbElementTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbElementTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiElementTreeDataRequestManager(this);

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

	// TODO: Review the commented out properties. [LK:2026-01-14]
	#mapItem(item: ElementTreeItemResponseModel): UmbElementTreeItemModel {
		return {
			unique: item.id,
			parent: {
				unique: item.parent ? item.parent.id : null,
				entityType: item.parent ? UMB_ELEMENT_ENTITY_TYPE : UMB_ELEMENT_ROOT_ENTITY_TYPE,
			},
			name: item.name,
			entityType: item.isFolder ? UMB_ELEMENT_FOLDER_ENTITY_TYPE : UMB_ELEMENT_ENTITY_TYPE,
			hasChildren: item.hasChildren,
			isTrashed: false, //item.isTrashed,
			isFolder: item.isFolder,
			documentType: {
				unique: item.documentType?.id ?? '',
				icon: item.documentType?.icon ?? 'icon-document',
				collection: null,
			},
			icon: item.isFolder ? 'icon-folder' : (item.documentType?.icon ?? 'icon-document'),
			createDate: item.createDate,
			variants: item.variants.map((variant) => {
				return {
					name: variant.name,
					culture: variant.culture || null,
					segment: null, // TODO: add segment to the backend API?
					state: variant.state,
					flags: [], //variant.flags,
				};
			}),
		};
	}
}
