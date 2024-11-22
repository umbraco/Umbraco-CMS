import {
	UMB_DATA_TYPE_ENTITY_TYPE,
	UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
	UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
} from '../entity.js';
import type { UmbDataTypeTreeItemModel } from './types.js';
import type {
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
	UmbTreeAncestorsOfRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { DataTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

let manifestPropertyEditorUis: Array<ManifestPropertyEditorUi> = [];

/**
 * A data source for a data type tree that fetches data from the server
 * @class UmbDataTypeTreeServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDataTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DataTypeTreeItemResponseModel,
	UmbDataTypeTreeItemModel
> {
	/**
	 * Creates an instance of UmbDataTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			getAncestorsOf,
			mapper,
		});
		umbExtensionsRegistry
			.byType('propertyEditorUi')
			.subscribe((manifestPropertyEditorUIs) => {
				manifestPropertyEditorUis = manifestPropertyEditorUIs;
			})
			.unsubscribe();
	}
}

const getRootItems = async (args: UmbTreeRootItemsRequestArgs) => {
	// eslint-disable-next-line local-rules/no-direct-api-import
	return DataTypeService.getTreeDataTypeRoot({
		foldersOnly: args.foldersOnly,
		skip: args.skip,
		take: args.take,
	});
};

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DataTypeService.getTreeDataTypeChildren({
			parentId: args.parent.unique,
			foldersOnly: args.foldersOnly,
			skip: args.skip,
			take: args.take,
		});
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DataTypeService.getTreeDataTypeAncestors({
		descendantId: args.treeItem.unique,
	});

const mapper = (item: DataTypeTreeItemResponseModel): UmbDataTypeTreeItemModel => {
	return {
		unique: item.id,
		parent: {
			unique: item.parent?.id || null,
			entityType: item.parent ? UMB_DATA_TYPE_ENTITY_TYPE : UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
		},
		icon: manifestPropertyEditorUis.find((ui) => ui.alias === item.editorUiAlias)?.meta.icon,
		name: item.name,
		entityType: item.isFolder ? UMB_DATA_TYPE_FOLDER_ENTITY_TYPE : UMB_DATA_TYPE_ENTITY_TYPE,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
	};
};
