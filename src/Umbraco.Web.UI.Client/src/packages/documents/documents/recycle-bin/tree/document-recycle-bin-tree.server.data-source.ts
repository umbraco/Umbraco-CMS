import type { UmbDocumentRecycleBinTreeItemModel } from './types.js';
import type { DocumentRecycleBinItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Document Recycle Bin tree that fetches data from the server
 * @export
 * @class UmbDocumentRecycleBinTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDocumentRecycleBinTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentRecycleBinItemResponseModel,
	UmbDocumentRecycleBinTreeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentRecycleBinTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentRecycleBinTreeServerDataSource
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
const getRootItems = () => DocumentResource.getRecycleBinDocumentRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentResource.getRecycleBinDocumentChildren({
			parentId: parentUnique,
		});
	}
};

const mapper = (item: DocumentRecycleBinItemResponseModel): UmbDocumentRecycleBinTreeItemModel => {
	return {
		id: item.id,
		parentId: item.parent ? item.parent.id : null,
		entityType: 'document-recycle-bin',
		hasChildren: item.hasChildren,
		isFolder: false,
		name: item.variants[0].name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
	};
};
