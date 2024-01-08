import { UMB_PARTIAL_VIEW_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../entity.js';
import { UmbServerPathUniqueSerializer } from '../../utils/server-path-unique-serializer.js';
import { UmbPartialViewTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import { FileSystemTreeItemPresentationModel, PartialViewResource } from '@umbraco-cms/backoffice/backend-api';
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

// eslint-disable-next-line local-rules/no-direct-api-import
const getRootItems = () => PartialViewResource.getTreePartialViewRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return PartialViewResource.getTreePartialViewChildren({
			parentPath: parentUnique,
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbPartialViewTreeItemModel => {
	const serializer = new UmbServerPathUniqueSerializer();

	return {
		unique: serializer.toUnique(item.path),
		parentUnique: item.parent ? serializer.toUnique(item.parent?.path) : null,
		entityType: item.isFolder ? UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE : UMB_PARTIAL_VIEW_ENTITY_TYPE,
		name: item.name,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		isContainer: false,
	};
};
