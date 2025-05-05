import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentBlueprintItemBaseModel, UmbDocumentBlueprintItemModel } from './types.js';
import { DocumentBlueprintService, DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DocumentBlueprintItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

/**
 * A data source for Document Blueprint items that fetches data from the server
 * @class UmbDocumentBlueprintItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentBlueprintItemServerDataSource extends UmbItemServerDataSourceBase<
	DocumentBlueprintItemResponseModel,
	UmbDocumentBlueprintItemModel
> {
	/**
	 * Creates an instance of UmbDocumentBlueprintItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentBlueprintItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
	}

	async getItemsByDocumentType(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		const { data, error } = await tryExecute(
			this,
			DocumentTypeService.getDocumentTypeByIdBlueprint({ path: { id: unique } }),
		);

		if (data) {
			const items: Array<UmbDocumentBlueprintItemBaseModel> = data.items.map((item) => ({
				entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
				unique: item.id,
				name: item.name,
			}));
			return { data: items };
		}

		return { error };
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => DocumentBlueprintService.getItemDocumentBlueprint({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

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
