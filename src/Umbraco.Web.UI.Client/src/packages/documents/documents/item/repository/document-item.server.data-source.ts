import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentItemModel } from './types.js';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

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
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => DocumentService.getItemDocument({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

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
