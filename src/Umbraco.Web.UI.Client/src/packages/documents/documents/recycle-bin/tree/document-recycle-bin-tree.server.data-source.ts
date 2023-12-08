import { UmbDocumentRecycleBinTreeItemModel } from './types.js';
import { DocumentResource, RecycleBinItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Document Recycle Bin tree that fetches data from the server
 * @export
 * @class UmbDocumentRecycleBinTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDocumentRecycleBinTreeServerDataSource extends UmbTreeServerDataSourceBase<
	RecycleBinItemResponseModel,
	UmbDocumentRecycleBinTreeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentRecycleBinTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentRecycleBinTreeServerDataSource
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
		return DocumentResource.getRecycleBinDocumentRoot({});
	} else {
		return DocumentResource.getRecycleBinDocumentChildren({
			parentId: parentUnique,
		});
	}
};

const mapper = (item: RecycleBinItemResponseModel): UmbDocumentRecycleBinTreeItemModel => {
	return {
		id: item.id!,
		parentId: item.parentId!,
		name: item.name!,
		type: 'document-recycle-bin',
		hasChildren: item.hasChildren!,
		isContainer: item.isContainer!,
	};
};
