import { UMB_PARTIAL_VIEW_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbPartialViewTreeItemModel } from './types.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { PartialViewResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the PartialView tree that fetches data from the server
 * @export
 * @class UmbPartialViewTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbPartialViewTreeServerDataSource extends UmbTreeServerDataSourceBase<
	FileSystemTreeItemPresentationModel,
	UmbPartialViewTreeItemModel
> {
	/**
	 * Creates an instance of UmbPartialViewTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbPartialViewTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			mapper,
		});
	}
}

const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	PartialViewResource.getTreePartialViewRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	const parentPath = new UmbServerFilePathUniqueSerializer().toServerPath(args.parentUnique);

	if (parentPath === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return PartialViewResource.getTreePartialViewChildren({
			parentPath,
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbPartialViewTreeItemModel => {
	const serializer = new UmbServerFilePathUniqueSerializer();

	return {
		unique: serializer.toUnique(item.path),
		parentUnique: item.parent ? serializer.toUnique(item.parent?.path) : null,
		entityType: item.isFolder ? UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE : UMB_PARTIAL_VIEW_ENTITY_TYPE,
		name: item.name,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		icon: item.isFolder ? undefined : 'icon-notepad',
	};
};
