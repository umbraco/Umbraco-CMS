import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

/**
 * A data source for Document Type items that fetches data from the server
 * @class UmbDocumentTypeItemServerDataSource
 * @augments {UmbItemServerDataSourceBase<DocumentTypeItemResponseModel, UmbDocumentTypeItemModel>}
 */
export class UmbDocumentTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	DocumentTypeItemResponseModel,
	UmbDocumentTypeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentTypeItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, { mapper });
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => DocumentTypeService.getItemDocumentType({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: DocumentTypeItemResponseModel): UmbDocumentTypeItemModel => {
	return {
		entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
		isElement: item.isElement,
		icon: item.icon,
		unique: item.id,
		name: item.name,
		description: item.description,
	};
};
