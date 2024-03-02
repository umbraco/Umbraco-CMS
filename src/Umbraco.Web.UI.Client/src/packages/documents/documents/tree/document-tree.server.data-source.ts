import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentTreeItemModel } from './types.js';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Document tree that fetches data from the server
 * @export
 * @class UmbDocumentTreeServerDataSource
 * @extends {UmbTreeServerDataSourceBase}
 */
export class UmbDocumentTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentTreeItemResponseModel,
	UmbDocumentTreeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTreeServerDataSource
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
	DocumentResource.getTreeDocumentRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parentUnique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentResource.getTreeDocumentChildren({
			parentId: args.parentUnique,
		});
	}
};

const mapper = (item: DocumentTreeItemResponseModel): UmbDocumentTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent ? item.parent.id : null,
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		noAccess: item.noAccess,
		isTrashed: item.isTrashed,
		hasChildren: item.hasChildren,
		isProtected: item.isProtected,
		documentType: {
			unique: item.documentType.id,
			icon: item.documentType.icon,
			collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
		},
		variants: item.variants.map((variant) => {
			return {
				name: variant.name,
				culture: variant.culture || null,
				state: variant.state,
			};
		}),
		name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
		isFolder: false,
	};
};
