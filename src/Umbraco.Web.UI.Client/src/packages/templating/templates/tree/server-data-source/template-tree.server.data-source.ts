import type { UmbTemplateTreeItemModel } from '../types.js';
import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbManagementApiTemplateTreeDataRequestManager } from './template-tree.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { NamedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeDataSource,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Template tree that fetches data from the server
 * @class UmbTemplateTreeServerDataSource
 */
export class UmbTemplateTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbTemplateTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiTemplateTreeDataRequestManager(this);

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

	#mapItem(item: NamedEntityTreeItemResponseModel): UmbTemplateTreeItemModel {
		return {
			unique: item.id,
			parent: {
				unique: item.parent ? item.parent.id : null,
				entityType: item.parent ? UMB_TEMPLATE_ENTITY_TYPE : UMB_TEMPLATE_ROOT_ENTITY_TYPE,
			},
			name: item.name,
			entityType: UMB_TEMPLATE_ENTITY_TYPE,
			hasChildren: item.hasChildren,
			isFolder: false,
			icon: 'icon-document-html',
		};
	}
}
