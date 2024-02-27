import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentTypeTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentTypeResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Document Type tree that fetches data from the server
 * @export
 * @class UmbDocumentTypeTreeServerDataSource
 * @extends {UmbTreeServerDataSourceBase}
 */
export class UmbDocumentTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentTypeTreeItemResponseModel,
	UmbDocumentTypeTreeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			mapper,
		});
	}
}

const getRootItems = (args: { skip: number; take: number }) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DocumentTypeResource.getTreeDocumentTypeRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: { parentUnique: string | null; skip: number; take: number }) => {
	if (args.parentUnique === null) {
		return getRootItems({ skip: args.skip, take: args.take });
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentTypeResource.getTreeDocumentTypeChildren({
			parentId: args.parentUnique,
			skip: args.skip,
			take: args.take,
		});
	}
};

const mapper = (item: DocumentTypeTreeItemResponseModel): UmbDocumentTypeTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent ? item.parent.id : null,
		name: item.name,
		entityType: item.isFolder ? UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE : UMB_DOCUMENT_TYPE_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: item.isFolder,
		icon: item.icon,
		isElement: item.isElement,
	};
};
