import {
	UMB_DATA_TYPE_ENTITY_TYPE,
	UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
	UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import type { UmbDataTypeTreeItemModel } from '../types.js';
import { UmbManagementApiDataTypeTreeDataRequestManager } from './data-type-tree.server.request-manager.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { DataTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeDataSource,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Data Type tree that fetches data from the server
 * @class UmbDataTypeTreeServerDataSource
 */
export class UmbDataTypeTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbDataTypeTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiDataTypeTreeDataRequestManager(this);
	#manifestPropertyEditorUis: Array<ManifestPropertyEditorUi> = [];

	constructor(host: UmbControllerHost) {
		super(host);

		umbExtensionsRegistry
			.byType('propertyEditorUi')
			.subscribe((manifestPropertyEditorUIs) => {
				this.#manifestPropertyEditorUis = manifestPropertyEditorUIs;
			})
			.unsubscribe();
	}

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

	#mapItem(item: DataTypeTreeItemResponseModel): UmbDataTypeTreeItemModel {
		return {
			unique: item.id,
			parent: {
				unique: item.parent?.id || null,
				entityType: item.parent ? UMB_DATA_TYPE_ENTITY_TYPE : UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
			},
			icon: this.#manifestPropertyEditorUis.find((ui) => ui.alias === item.editorUiAlias)?.meta.icon,
			name: item.name,
			entityType: item.isFolder ? UMB_DATA_TYPE_FOLDER_ENTITY_TYPE : UMB_DATA_TYPE_ENTITY_TYPE,
			isFolder: item.isFolder,
			hasChildren: item.hasChildren,
		};
	}
}
