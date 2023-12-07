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
			getChildrenOf,
			mapper,
		});
	}
}

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return PartialViewResource.getTreePartialViewRoot({});
	} else {
		return PartialViewResource.getTreePartialViewChildren({
			path: parentUnique,
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbPartialViewTreeItemModel => {
	return {
		path: item.path!,
		name: item.name!,
		type: 'partial-view',
		isFolder: item.isFolder!,
		hasChildren: item.hasChildren!,
		isContainer: false,
	};
};
