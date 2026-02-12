import {
	UMB_STYLESHEET_ENTITY_TYPE,
	UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
	UMB_STYLESHEET_ROOT_ENTITY_TYPE,
} from '../entity.js';
import type { UmbStylesheetTreeItemModel } from './types.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { StylesheetService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

/**
 * A data source for the Stylesheet tree that fetches data from the server
 * @class UmbStylesheetTreeServerDataSource
 * @implements {UmbTreeServerDataSourceBase}
 */
export class UmbStylesheetTreeServerDataSource extends UmbTreeServerDataSourceBase<
	FileSystemTreeItemPresentationModel,
	UmbStylesheetTreeItemModel
> {
	/**
	 * Creates an instance of UmbStylesheetTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStylesheetTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			getAncestorsOf,
			mapper,
		});
	}
}

const getRootItems = async (args: UmbTreeRootItemsRequestArgs) => {
	const { skip = 0, take = 100 } = (args.paging ?? {}) as UmbOffsetPaginationRequestModel;
	// eslint-disable-next-line local-rules/no-direct-api-import
	const { data, ...rest } = await StylesheetService.getTreeStylesheetRoot({
		query: { skip, take },
	});
	return { data: { ...data, totalBefore: 0, totalAfter: Math.max(data.total - data.items.length, 0) }, ...rest };
};

const getChildrenOf = async (args: UmbTreeChildrenOfRequestArgs) => {
	const parentPath = new UmbServerFilePathUniqueSerializer().toServerPath(args.parent.unique);

	if (parentPath === null) {
		return getRootItems(args);
	} else {
		const { skip = 0, take = 100 } = (args.paging ?? {}) as UmbOffsetPaginationRequestModel;
		// eslint-disable-next-line local-rules/no-direct-api-import
		const { data, ...rest } = await StylesheetService.getTreeStylesheetChildren({
			query: { parentPath, skip, take },
		});
		return { data: { ...data, totalBefore: 0, totalAfter: Math.max(data.total - data.items.length, 0) }, ...rest };
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) => {
	const descendantPath = new UmbServerFilePathUniqueSerializer().toServerPath(args.treeItem.unique);
	if (!descendantPath) throw new Error('Descendant path is not available');

	// eslint-disable-next-line local-rules/no-direct-api-import
	return StylesheetService.getTreeStylesheetAncestors({
		query: { descendantPath },
	});
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbStylesheetTreeItemModel => {
	const serializer = new UmbServerFilePathUniqueSerializer();

	return {
		unique: serializer.toUnique(item.path),
		entityType: item.isFolder ? UMB_STYLESHEET_FOLDER_ENTITY_TYPE : UMB_STYLESHEET_ENTITY_TYPE,
		parent: {
			unique: item.parent ? serializer.toUnique(item.parent.path) : null,
			entityType: item.parent ? UMB_STYLESHEET_ENTITY_TYPE : UMB_STYLESHEET_ROOT_ENTITY_TYPE,
		},
		name: item.name,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		icon: item.isFolder ? undefined : 'icon-palette',
	};
};
