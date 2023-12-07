import { UmbStylesheetTreeItemModel } from './types.js';
import { FileSystemTreeItemPresentationModel, StylesheetResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Stylesheet tree that fetches data from the server
 * @export
 * @class UmbStylesheetTreeServerDataSource
 * @implements {UmbTreeDataSource}
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
			getChildrenOf,
			mapper,
		});
	}
}

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return StylesheetResource.getTreeStylesheetRoot({});
	} else {
		return StylesheetResource.getTreeStylesheetChildren({
			path: parentUnique,
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbStylesheetTreeItemModel => {
	return {
		path: item.path!,
		name: item.name!,
		type: 'stylesheet',
		isFolder: item.isFolder!,
		hasChildren: item.hasChildren!,
		isContainer: false,
	};
};
