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
	#dataSourceApi?: UmbPickerDataSource;
	#dataSourceConfig?: UmbConfigCollectionModel | undefined;

	#dataSourceApiContext = new UmbEntityDataPickerDataSourceApiContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS);
	}

	/**
	 * Sets the data source API for the input context and updates the modal token accordingly.
	 * @param {UmbPickerDataSource | undefined} api The data source API to set for the input context.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	setDataSourceApi(api: UmbPickerDataSource | undefined) {
		if (api) {
			this.#dataSourceApi = api;
			api.setConfig?.(this.#dataSourceConfig);
			this.#dataSourceApiContext.setDataSourceApi(api);
			this.#setModalToken();
		} else {
			this.#dataSourceApi = undefined;
			this.#dataSourceApiContext.setDataSourceApi(undefined);
		}
	}

	/**
	 * Gets the data source API for the input context.
	 * @returns {(UmbPickerDataSource | undefined)} The data source API for the input context.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	getDataSourceApi(): UmbPickerDataSource | undefined {
		return this.#dataSourceApi;
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
