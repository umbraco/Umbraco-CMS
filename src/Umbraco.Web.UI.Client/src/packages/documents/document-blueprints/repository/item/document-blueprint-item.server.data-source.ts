import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentBlueprintItemModel } from './types.js';
import type { DocumentBlueprintItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentBlueprintResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Document Blueprint items that fetches data from the server
 * @export
 * @class UmbDocumentBlueprintItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentBlueprintItemServerDataSource extends UmbItemServerDataSourceBase<
	DocumentBlueprintItemResponseModel,
	UmbDocumentBlueprintItemModel
> {
	/**
	 * Creates an instance of UmbDocumentBlueprintItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentBlueprintItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => DocumentBlueprintResource.getItemDocumentBlueprint({ id: uniques });

const mapper = (item: DocumentBlueprintItemResponseModel): UmbDocumentBlueprintItemModel => {
	return {
		entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
		documentType: {
			unique: item.documentType.id,
			icon: item.documentType.icon,
			collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
		},
	};
};
