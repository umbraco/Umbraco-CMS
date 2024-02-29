import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentItemModel } from './types.js';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Document items that fetches data from the server
 * @export
 * @class UmbDocumentItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentItemServerDataSource extends UmbItemServerDataSourceBase<
	DocumentItemResponseModel,
	UmbDocumentItemModel
> {
	/**
	 * Creates an instance of UmbDocumentItemServerDataSource.
	 * @param {UmbControllerHost} host
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
const getItems = (uniques: Array<string>) => DocumentResource.getItemDocument({ id: uniques });

const mapper = (item: DocumentItemResponseModel): UmbDocumentItemModel => {
	return {
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		unique: item.id,
		isTrashed: item.isTrashed,
		isProtected: item.isProtected,
		documentType: {
			unique: item.documentType.id,
			icon: item.documentType.icon,
			collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
		},
		variants: item.variants.map((variant) => {
			return {
				culture: variant.culture || null,
				name: variant.name,
				state: variant.state,
			};
		}),
		name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
	};
};
