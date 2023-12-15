import { UmbStylesheetTreeItemModel } from './types.js';
import { FileSystemTreeItemPresentationModel, StylesheetResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Stylesheet tree that fetches data from the server
 * @export
 * @class UmbStylesheetTreeServerDataSource
 * @implements {UmbTreeServerDataSourceBase}
 */
export class UmbStylesheetTreeServerDataSource extends UmbTreeServerDataSourceBase<
	FileSystemTreeItemPresentationModel,
	UmbStylesheetTreeItemModel
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
const getRootItems = () => StylesheetResource.getTreeStylesheetRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return StylesheetResource.getTreeStylesheetChildren({
			path: parentUnique,
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbStylesheetTreeItemModel => {
	return {
		path: item.path,
		name: item.name,
		entityType: 'stylesheet',
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		isContainer: false,
	};
};
