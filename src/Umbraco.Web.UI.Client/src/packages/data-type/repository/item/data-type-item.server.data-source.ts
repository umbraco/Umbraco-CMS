import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbDataTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

let manifestPropertyEditorUis: Array<ManifestPropertyEditorUi> = [];

/**
 * A server data source for Data Type items
 * @class UmbDataTypeItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDataTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	DataTypeItemResponseModel,
	UmbDataTypeItemModel
> {
	/**
	 * Creates an instance of UmbDataTypeItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeItemServerDataSource
	 */

	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});

		umbExtensionsRegistry
			.byType('propertyEditorUi')
			.subscribe((manifestPropertyEditorUIs) => {
				manifestPropertyEditorUis = manifestPropertyEditorUIs;
			})
			.unsubscribe();
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => DataTypeService.getItemDataType({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: DataTypeItemResponseModel): UmbDataTypeItemModel => {
	return {
		entityType: UMB_DATA_TYPE_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
		propertyEditorSchemaAlias: item.editorAlias,
		propertyEditorUiAlias: item.editorUiAlias || '', // TODO: why can this be undefined or null on the server?
		icon: manifestPropertyEditorUis.find((ui) => ui.alias === item.editorUiAlias)?.meta.icon,
	};
};
