import { UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS } from '../picker-collection/constants.js';
import { UMB_ENTITY_DATA_PICKER_TREE_ALIAS } from '../picker-tree/constants.js';
import { UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS } from '../picker-search/constants.js';
import { UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbEntityDataPickerItemModel } from '../picker-item/types.js';
import { UmbEntityDataPickerDataSourceApiContext } from './entity-data-picker-data-source.context.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	type UmbCollectionItemPickerModalData,
	type UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';
import type {
	ManifestPropertyEditorDataSource,
	UmbPickerPropertyEditorDataSource,
	UmbPickerPropertyEditorTreeDataSource,
} from '@umbraco-cms/backoffice/data-type';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	umbConfirmModal,
	UmbModalToken,
	umbOpenModal,
	type UmbPickerModalData,
	type UmbPickerModalValue,
} from '@umbraco-cms/backoffice/modal';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import {
	UMB_TREE_PICKER_MODAL_ALIAS,
	type UmbTreePickerModalData,
	type UmbTreePickerModalValue,
} from '@umbraco-cms/backoffice/tree';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export class UmbEntityDataPickerInputContext extends UmbControllerBase {
	#itemManager = new UmbRepositoryItemsManager<UmbEntityDataPickerItemModel>(
		this,
		UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS,
	);
	public readonly selection = this.#itemManager.uniques;
	public readonly selectedItems = this.#itemManager.items;
	public readonly statuses = this.#itemManager.statuses;

	#dataSourceAlias?: string;
	#dataSourceApiInitializer?: UmbExtensionApiInitializer<ManifestPropertyEditorDataSource>;

	#pickerModalToken?: UmbModalToken<UmbPickerModalData<UmbEntityDataPickerItemModel>, UmbPickerModalValue>;
	#dataSourceApiContext = new UmbEntityDataPickerDataSourceApiContext(this);

	/**
	 * Define a minimum amount of selected items in this input, for this input to be valid.
	 * @returns {number} The minimum number of items required.
	 */
	public get max() {
		return this._max;
	}
	public set max(value) {
		this._max = value === undefined ? Infinity : value;
	}
	private _max = Infinity;

	/**
	 * Define a maximum amount of selected items in this input, for this input to be valid.
	 * @returns {number} The minimum number of items required.
	 */
	public get min() {
		return this._min;
	}
	public set min(value) {
		this._min = value === undefined ? 0 : value;
	}
	private _min = 0;

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
	 * @param {(UmbPropertyEditorConfigCollection | undefined)} config The data source config.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	setDataSourceConfig(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#dataSourceApiContext.setConfig(config);
	}

	/**
	 * Gets the data source config for the input context.
	 * @returns {(UmbPropertyEditorConfigCollection | undefined)} The data source config.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	getDataSourceConfig(): UmbPropertyEditorConfigCollection | undefined {
		return this.#dataSourceApiContext.getConfig();
	}

	getSelection() {
		return this.#itemManager.getUniques();
	}

	setSelection(selection: Array<string | null>) {
		// Note: Currently we do not support picking root item. So we filter out null values:
		this.#itemManager.setUniques(selection.filter((value) => value !== null) as Array<string>);
	}

	async openPicker() {
		await this.#itemManager.init;

		if (!this.#pickerModalToken) {
			console.warn('No modal token is set for the picker, cannot open modal.');
			return;
		}

		const modalPresetData: UmbPickerModalData<UmbEntityDataPickerItemModel> = {
			multiple: this._max === 1 ? false : true,
		};

		const modalPresetValue: UmbPickerModalValue = {
			selection: this.getSelection(),
		};

		const modalValue = (await umbOpenModal(this, this.#pickerModalToken, {
			data: modalPresetData,
			value: modalPresetValue,
		}).catch(() => undefined)) as UmbPickerModalValue | undefined;

		if (!modalValue) return;

		this.setSelection(modalValue.selection);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}

	async requestRemoveItem(unique: string) {
		const item = this.#itemManager.getItems().find((item) => item.unique === unique);
		const name = item?.name ?? '#general_notFound';

		await umbConfirmModal(this, {
			color: 'danger',
			headline: `#actions_remove ${name}?`,
			content: `#defaultdialogs_confirmremove ${name}?`,
			confirmLabel: '#actions_remove',
		});

		this.#removeItem(unique);
	}

	#removeItem(unique: string) {
		const newSelection = this.getSelection().filter((value) => value !== unique);
		this.setSelection(newSelection);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}

	#createDataSourceApi(dataSourceAlias: string | undefined) {
		if (!dataSourceAlias) {
			this.#dataSourceApiInitializer?.destroy();
			return;
		}

		this.#dataSourceApiInitializer = new UmbExtensionApiInitializer<ManifestPropertyEditorDataSource>(
			this,
			umbExtensionsRegistry,
			dataSourceAlias,
			[this._host],
			(permitted, ctrl) => {
				if (!permitted) {
					// TODO: clean up if not permitted
					return;
				}

				// TODO: Check if it is a picker data source
				const dataSourceApi = ctrl.api as UmbPickerPropertyEditorDataSource;
				dataSourceApi.setConfig?.(this.getDataSourceConfig());

				this.#dataSourceApiContext.setDataSourceApi(dataSourceApi);

				// TODO: can we find a better way to check if the api is a tree or collection data source?
				const isTreeDataSource =
					typeof dataSourceApi.requestTreeRoot === 'function' && typeof dataSourceApi.requestTreeItemsOf === 'function';

				// Choose the picker type based on what the data source supports
				if (isTreeDataSource) {
					this.#pickerModalToken = this.#createTreeItemPickerModalToken(dataSourceApi);
				} else {
					this.#pickerModalToken = this.#createCollectionItemPickerModalToken(dataSourceApi);
				}
			},
		);
	}

	#createTreeItemPickerModalToken(api: UmbPickerPropertyEditorTreeDataSource) {
		// TODO: can we find a better way to check if the api supports search?
		const supportsSearch = typeof api.search === 'function';

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

	#createCollectionItemPickerModalToken(api: UmbPickerPropertyEditorTreeDataSource) {
		const supportsSearch = typeof api.search === 'function';

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
