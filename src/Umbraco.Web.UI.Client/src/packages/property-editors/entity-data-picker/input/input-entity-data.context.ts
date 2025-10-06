import { UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS } from '../picker-collection/constants.js';
import { UMB_ENTITY_DATA_PICKER_TREE_ALIAS } from '../picker-tree/constants.js';
import { UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS } from '../picker-search/constants.js';
import { UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbEntityDataPickerItemModel } from '../picker-item/types.js';
import { UmbEntityDataPickerDataSourceApiContext } from './entity-data-picker-data-source.context.js';
import {
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	type UmbCollectionItemPickerModalData,
	type UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';
import type {
	ManifestPropertyEditorDataSource,
	UmbPickerPropertyEditorCollectionDataSource,
	UmbPickerPropertyEditorDataSource,
	UmbPickerPropertyEditorTreeDataSource,
	UmbPropertyEditorDataSourceConfigModel,
} from '@umbraco-cms/backoffice/data-type';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalToken, type UmbPickerModalData } from '@umbraco-cms/backoffice/modal';
import {
	UMB_TREE_PICKER_MODAL_ALIAS,
	type UmbTreePickerModalData,
	type UmbTreePickerModalValue,
} from '@umbraco-cms/backoffice/tree';
import { isPickerPropertyEditorTreeDataSource } from 'src/packages/data-type/property-editor-data-source/extension/is-picker-property-editor-tree-data-source.guard.js';
import { isPickerPropertyEditorCollectionDataSource } from 'src/packages/data-type/property-editor-data-source/extension/is-picker-property-editor-collection-data-source.guard copy.js';
import { isPickerPropertyEditorSearchableDataSource } from 'src/packages/data-type/property-editor-data-source/extension/is-picker-property-editor-searchable.data-source.guard.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbEntityDataPickerInputContext extends UmbPickerInputContext<UmbEntityDataPickerItemModel> {
	#dataSourceAlias?: string;
	#dataSourceApiInitializer?: UmbExtensionApiInitializer<ManifestPropertyEditorDataSource>;
	#dataSourceApi?: UmbPickerPropertyEditorDataSource;
	#dataSourceConfig?: UmbPropertyEditorDataSourceConfigModel | undefined;

	#dataSourceApiContext = new UmbEntityDataPickerDataSourceApiContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS);
	}

	/**
	 * Sets the data source alias for the input context.
	 * @param {(string | undefined)} alias
	 * @memberof UmbEntityDataPickerInputContext
	 */
	setDataSourceAlias(alias: string | undefined) {
		this.#dataSourceAlias = alias;
		this.#createDataSourceApi(alias);
	}

	/**
	 * Gets the data source alias for the input context.
	 * @returns {(string | undefined)} The data source alias.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	getDataSourceAlias(): string | undefined {
		return this.#dataSourceAlias;
	}

	/**
	 * Sets the data source config for the input context.
	 * @param {(UmbPropertyEditorDataSourceConfigModel | undefined)} config The data source config.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	setDataSourceConfig(config: UmbPropertyEditorDataSourceConfigModel | undefined) {
		this.#dataSourceConfig = config;
	}

	/**
	 * Gets the data source config for the input context.
	 * @returns {(UmbPropertyEditorDataSourceConfigModel | undefined)} The data source config.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	getDataSourceConfig(): UmbPropertyEditorDataSourceConfigModel | undefined {
		return this.#dataSourceConfig;
	}

	override async openPicker(pickerData?: Partial<UmbPickerModalData<UmbEntityDataPickerItemModel>>) {
		this.modalAlias = this.#getModalToken();
		await super.openPicker(pickerData);
	}

	#createDataSourceApi(dataSourceAlias: string | undefined) {
		if (!dataSourceAlias) {
			this.#dataSourceApiInitializer?.destroy();
			return;
		}

		this.#dataSourceApiInitializer = new UmbExtensionApiInitializer<
			ManifestPropertyEditorDataSource,
			UmbExtensionApiInitializer<ManifestPropertyEditorDataSource>,
			UmbPickerPropertyEditorDataSource
		>(this, umbExtensionsRegistry, dataSourceAlias, [this._host], (permitted, ctrl) => {
			if (!permitted) {
				// TODO: clean up if not permitted
				return;
			}

			// TODO: Check if it is a picker data source
			this.#dataSourceApi = ctrl.api as UmbPickerPropertyEditorDataSource;
			this.#dataSourceApi.setConfig?.(this.#dataSourceConfig);

			this.#dataSourceApiContext.setDataSourceApi(this.#dataSourceApi);
		});
	}

	#getModalToken() {
		if (!this.#dataSourceApi) return;

		const dataSourceApi = this.#dataSourceApi;

		const isTreeDataSource = isPickerPropertyEditorTreeDataSource(dataSourceApi);
		const isCollectionDataSource = isPickerPropertyEditorCollectionDataSource(dataSourceApi);

		// Choose the picker type based on what the data source supports
		if (isTreeDataSource) {
			return this.#createTreeItemPickerModalToken(dataSourceApi);
		} else if (isCollectionDataSource) {
			return this.#createCollectionItemPickerModalToken(dataSourceApi);
		} else {
			throw new Error('The data source API is not a supported type of picker data source.');
		}
	}

	#createTreeItemPickerModalToken(api: UmbPickerPropertyEditorTreeDataSource) {
		const supportsSearch = isPickerPropertyEditorSearchableDataSource(api);

		return new UmbModalToken<UmbTreePickerModalData<UmbEntityDataPickerItemModel>, UmbTreePickerModalValue>(
			UMB_TREE_PICKER_MODAL_ALIAS,
			{
				modal: {
					type: 'sidebar',
					size: 'small',
				},
				data: {
					treeAlias: UMB_ENTITY_DATA_PICKER_TREE_ALIAS,
					expandTreeRoot: true,
					search: supportsSearch
						? {
								providerAlias: UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS,
							}
						: undefined,
				},
			},
		);
	}

	#createCollectionItemPickerModalToken(api: UmbPickerPropertyEditorCollectionDataSource) {
		const supportsSearch = isPickerPropertyEditorSearchableDataSource(api);

		return new UmbModalToken<
			UmbCollectionItemPickerModalData<UmbEntityDataPickerItemModel>,
			UmbCollectionItemPickerModalValue
		>(UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS, {
			modal: {
				type: 'sidebar',
				size: 'small',
			},
			data: {
				collection: {
					menuAlias: UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
				},
				search: supportsSearch
					? {
							providerAlias: UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS,
						}
					: undefined,
			},
		});
	}
}
