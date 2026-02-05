import {
	UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
	UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
} from '../picker-collection/constants.js';
import { UMB_ENTITY_DATA_PICKER_TREE_ALIAS } from '../picker-tree/constants.js';
import { UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS } from '../picker-search/constants.js';
import { UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UmbEntityDataPickerDataSourceApiContext } from './entity-data-picker-data-source.context.js';
import {
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	type UmbCollectionItemPickerModalData,
	type UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';
import type { ManifestDataSource } from '@umbraco-cms/backoffice/data-source';
import type { ManifestPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor-data-source';

type DataSourceManifest = ManifestPropertyEditorDataSource | ManifestDataSource;
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalToken, type UmbPickerModalData } from '@umbraco-cms/backoffice/modal';
import {
	UMB_TREE_PICKER_MODAL_ALIAS,
	type UmbTreeItemModel,
	type UmbTreePickerModalData,
	type UmbTreePickerModalValue,
} from '@umbraco-cms/backoffice/tree';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	isPickerCollectionDataSource,
	isPickerSearchableDataSource,
	isPickerTreeDataSource,
	type UmbPickerCollectionDataSource,
	type UmbPickerDataSource,
	type UmbPickerTreeDataSource,
} from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export class UmbEntityDataPickerInputContext extends UmbPickerInputContext<
	UmbItemModel,
	UmbItemModel | UmbTreeItemModel
> {
	#dataSourceAlias?: string;
	#dataSourceApiInitializer?: UmbExtensionApiInitializer<DataSourceManifest>;
	#dataSourceApi?: UmbPickerDataSource;
	#dataSourceConfig?: UmbConfigCollectionModel | undefined;

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
	setDataSourceConfig(config: UmbConfigCollectionModel | undefined) {
		this.#dataSourceConfig = config;

		if (this.#dataSourceApi) {
			this.#dataSourceApi.setConfig?.(config);
		}
	}

	/**
	 * Gets the data source config for the input context.
	 * @returns {(UmbPropertyEditorDataSourceConfigModel | undefined)} The data source config.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	getDataSourceConfig(): UmbConfigCollectionModel | undefined {
		return this.#dataSourceConfig;
	}

	override async openPicker(pickerData?: Partial<UmbPickerModalData<UmbItemModel>>) {
		// TODO: investigate type issues
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.modalAlias = this.getModalAlias();
		await super.openPicker(pickerData);
	}

	#createDataSourceApi(dataSourceAlias: string | undefined) {
		if (!dataSourceAlias) {
			this.#dataSourceApiInitializer?.destroy();
			return;
		}

		this.#dataSourceApiInitializer = new UmbExtensionApiInitializer<
			DataSourceManifest,
			UmbExtensionApiInitializer<DataSourceManifest>,
			UmbPickerDataSource
		>(this, umbExtensionsRegistry, dataSourceAlias, [this._host], (permitted, ctrl) => {
			if (!permitted) {
				// TODO: clean up if not permitted
				return;
			}

			// TODO: Check if it is a picker data source
			this.#dataSourceApi = ctrl.api as UmbPickerDataSource;
			this.#dataSourceApi.setConfig?.(this.#dataSourceConfig);

			this.#dataSourceApiContext.setDataSourceApi(this.#dataSourceApi);
			this.#setModalToken();
		});
	}

	#setModalToken() {
		if (!this.#dataSourceApi) return;

		const dataSourceApi = this.#dataSourceApi;

		const isTreeDataSource = isPickerTreeDataSource(dataSourceApi);
		const isCollectionDataSource = isPickerCollectionDataSource(dataSourceApi);

		// Choose the picker type based on what the data source supports
		if (isTreeDataSource) {
			const token = this.#createTreeItemPickerModalToken(dataSourceApi);
			this.setModalAlias(token);
		} else if (isCollectionDataSource) {
			const token = this.#createCollectionItemPickerModalToken(dataSourceApi);
			this.setModalAlias(token);
		} else {
			throw new Error('The data source API is not a supported type of picker data source.');
		}
	}

	#createTreeItemPickerModalToken(api: UmbPickerTreeDataSource) {
		const supportsSearch = isPickerSearchableDataSource(api);

		return new UmbModalToken<UmbTreePickerModalData<UmbItemModel | UmbTreeItemModel>, UmbTreePickerModalValue>(
			UMB_TREE_PICKER_MODAL_ALIAS,
			{
				modal: {
					type: 'sidebar',
					size: 'small',
				},
				data: {
					treeAlias: UMB_ENTITY_DATA_PICKER_TREE_ALIAS,
					hideTreeRoot: true,
					// TODO: make specific pickable filter for tree to avoid type issues
					pickableFilter: api.treePickableFilter as ((item: UmbItemModel | UmbTreeItemModel) => boolean) | undefined,
					search: supportsSearch
						? {
								providerAlias: UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS,
								pickableFilter: api.searchPickableFilter,
							}
						: undefined,
				},
			},
		);
	}

	#createCollectionItemPickerModalToken(api: UmbPickerCollectionDataSource) {
		const supportsSearch = isPickerSearchableDataSource(api);

		return new UmbModalToken<UmbCollectionItemPickerModalData<UmbItemModel>, UmbCollectionItemPickerModalValue>(
			UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
			{
				modal: {
					type: 'sidebar',
					size: 'medium',
				},
				data: {
					collection: {
						alias: UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
						menuAlias: UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
					},
					// TODO: make specific pickable filter for collection to avoid type issues
					pickableFilter: api.collectionPickableFilter,
					search: supportsSearch
						? {
								providerAlias: UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS,
								pickableFilter: api.searchPickableFilter,
							}
						: undefined,
				},
			},
		);
	}
}
