import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentBlueprintItemModel } from './types.js';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { CancelablePromise, DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for Document Blueprint items that fetches data from the server
 * @export
 * @class UmbDocumentBlueprintItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentBlueprintItemServerDataSource extends UmbItemServerDataSourceBase<
	DocumentItemResponseModel,
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
const getItems = (uniques: Array<string>) =>
	DocumentBlueprintService.getItemDocumentBlueprint({ id: uniques }) as unknown as CancelablePromise<
		DocumentItemResponseModel[]
	>;

const mapper = (item: DocumentItemResponseModel): UmbDocumentBlueprintItemModel => {
	return {
		entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
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
