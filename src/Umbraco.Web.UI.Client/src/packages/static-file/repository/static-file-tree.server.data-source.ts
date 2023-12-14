import { UMB_STATIC_FILE_ENTITY_TYPE } from '../entity.js';
import { UmbStaticFileTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import { StaticFileResource, type FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Static File tree that fetches data from the server
 * @export
 * @class UmbStaticFileTreeServerDataSource
 * @implements {UmbTreeServerDataSourceBase}
 */
export class UmbStaticFileTreeServerDataSource extends UmbTreeServerDataSourceBase<
	FileSystemTreeItemPresentationModel,
	UmbStaticFileTreeItemModel
> {
	/**
	 * Creates an instance of UmbStylesheetTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetTreeServerDataSource
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
const getRootItems = () => StaticFileResource.getTreeStaticFileRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return StaticFileResource.getTreeStaticFileChildren({
			path: parentUnique,
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbStaticFileTreeItemModel => {
	return {
		path: item.path,
		name: item.name,
		entityType: UMB_STATIC_FILE_ENTITY_TYPE,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		isContainer: false,
	};
};
