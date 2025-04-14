import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentItemModel } from './types.js';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Document items that fetches data from the server
 * @class UmbDocumentItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentItemServerDataSource extends UmbItemServerDataSourceBase<
	DocumentItemResponseModel,
	UmbDocumentItemModel
> {
	/**
	 * Creates an instance of UmbDocumentItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => DocumentService.getItemDocument({ query: { id: uniques } });

const mapper = (item: DocumentItemResponseModel): UmbDocumentItemModel => {
	return {
		documentType: {
			collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
			icon: item.documentType.icon,
			unique: item.documentType.id,
		},
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isProtected: item.isProtected,
		isTrashed: item.isTrashed,
		name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
		parent: item.parent ? { unique: item.parent.id } : null,
		unique: item.id,
		variants: item.variants.map((variant) => {
			return {
				culture: variant.culture || null,
				name: variant.name,
				state: variant.state,
			};
		}),
	};
};
