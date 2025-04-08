import type { UmbDataTypeCollectionFilterModel } from '../types.js';
import type { UmbDataTypeItemModel } from '../../repository/index.js';
import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

/**
 * A data source that fetches the data-type collection data from the server.
 * @class UmbDataTypeCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbDataTypeCollectionServerDataSource implements UmbCollectionDataSource<UmbDataTypeItemModel> {
	#host: UmbControllerHost;
	#manifestPropertyEditorUis: Array<ManifestPropertyEditorUi> = [];

	/**
	 * Creates an instance of UmbDataTypeCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @DataTypeof UmbDataTypeCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
		umbExtensionsRegistry
			.byType('propertyEditorUi')
			.subscribe((manifestPropertyEditorUIs) => {
				this.#manifestPropertyEditorUis = manifestPropertyEditorUIs;
			})
			.unsubscribe();
	}

	/**
	 * Gets the DataType collection filtered by the given filter.
	 * @param {UmbDataTypeCollectionFilterModel} filter
	 * @returns {*}
	 * @DataTypeof UmbDataTypeCollectionServerDataSource
	 */
	async getCollection(filter: UmbDataTypeCollectionFilterModel) {
		const { data, error } = await tryExecute(this.#host, DataTypeService.getFilterDataType(filter));

		if (error) {
			return { error };
		}

		if (!data) {
			return { data: { items: [], total: 0 } };
		}

		const { items, total } = data;

		const mappedItems: Array<UmbDataTypeItemModel> = items.map((item: DataTypeItemResponseModel) => {
			const dataTypeDetail: UmbDataTypeItemModel = {
				entityType: UMB_DATA_TYPE_ENTITY_TYPE,
				unique: item.id,
				name: item.name,
				propertyEditorSchemaAlias: item.editorAlias,
				propertyEditorUiAlias: item.editorUiAlias!,
				icon: this.#manifestPropertyEditorUis.find((ui) => ui.alias === item.editorUiAlias!)?.meta.icon,
			};

			return dataTypeDetail;
		});

		return { data: { items: mappedItems, total } };
	}
}
