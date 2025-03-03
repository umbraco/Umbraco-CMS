import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
		super(host, { getItems, mapper });
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => DocumentTypeService.getItemDocumentType({ id: uniques });

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
