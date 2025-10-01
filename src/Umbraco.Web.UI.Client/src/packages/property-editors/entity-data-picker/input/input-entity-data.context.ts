import type { UmbEntityDataItemModel } from '../types.js';
import { UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS } from '../picker-collection/constants.js';
import { UMB_ENTITY_DATA_PICKER_TREE_ALIAS } from '../picker-tree/constants.js';
import { UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS } from '../picker-search/constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	type UmbCollectionItemPickerModalData,
	type UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';
import type {
	ManifestPickerPropertyEditorCollectionDataSource,
	ManifestPickerPropertyEditorTreeDataSource,
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

type ManifestDataSourceType =
	| ManifestPickerPropertyEditorTreeDataSource
	| ManifestPickerPropertyEditorCollectionDataSource;

export class UmbEntityDataPickerInputContext extends UmbControllerBase {
	#itemManager = new UmbRepositoryItemsManager<UmbEntityDataItemModel>(this);
	public readonly selection = this.#itemManager.uniques;
	public readonly selectedItems = this.#itemManager.items;
	public readonly statuses = this.#itemManager.statuses;

	#dataSourceAlias?: string;
	#dataSourceConfig?: UmbPropertyEditorConfigCollection;
	#dataSourceApiInitializer?: UmbExtensionApiInitializer<ManifestDataSourceType>;

	#pickerModalToken?: UmbModalToken<UmbPickerModalData<UmbEntityDataItemModel>, UmbPickerModalValue>;

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
		this.#dataSourceConfig = config;
	}

	/**
	 * Gets the data source config for the input context.
	 * @returns {(UmbPropertyEditorConfigCollection | undefined)} The data source config.
	 * @memberof UmbEntityDataPickerInputContext
	 */
	getDataSourceConfig(): UmbPropertyEditorConfigCollection | undefined {
		return this.#dataSourceConfig;
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

		const modalPresetData: UmbPickerModalData<UmbEntityDataItemModel> = {
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

		this.#dataSourceApiInitializer = new UmbExtensionApiInitializer<ManifestDataSourceType>(
			this,
			umbExtensionsRegistry,
			dataSourceAlias,
			[this._host],
			(permitted, ctrl) => {
				if (!permitted) {
					this.#itemManager.setItemRepository(undefined);
					return;
				}

				this.provideContext('testy', ctrl.api);

				const itemRepository = ctrl.api.item;

				if (!itemRepository) {
					console.warn(
						`The data source with alias ${dataSourceAlias} did not provide an item repository for the picker to use.`,
					);
				}

				// TODO: make it possible to also provide an item repository alias
				this.#itemManager.setItemRepository(itemRepository);

				// Choose the picker type based on what the data source supports
				if (ctrl.api.tree) {
					this.#pickerModalToken = this.#createTreeItemPickerModalToken(ctrl.api);
				} else {
					this.#pickerModalToken = this.#createCollectionItemPickerModalToken(ctrl.api);
				}
			},
		);
	}

	#createTreeItemPickerModalToken(api: UmbPickerPropertyEditorTreeDataSource) {
		return new UmbModalToken<UmbTreePickerModalData<UmbEntityDataItemModel>, UmbTreePickerModalValue>(
			UMB_TREE_PICKER_MODAL_ALIAS,
			{
				modal: {
					type: 'sidebar',
					size: 'small',
				},
				data: {
					treeAlias: UMB_ENTITY_DATA_PICKER_TREE_ALIAS,
					expandTreeRoot: true,
					search: api.search
						? {
								providerAlias: UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS,
							}
						: undefined,
				},
			},
		);
	}

	#createCollectionItemPickerModalToken(api: UmbPickerPropertyEditorTreeDataSource) {
		return new UmbModalToken<
			UmbCollectionItemPickerModalData<UmbEntityDataItemModel>,
			UmbCollectionItemPickerModalValue
		>(UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS, {
			modal: {
				type: 'sidebar',
				size: 'small',
			},
			data: {
				collectionMenuAlias: UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
				search: api.search
					? {
							providerAlias: UMB_ENTITY_DATA_PICKER_SEARCH_PROVIDER_ALIAS,
						}
					: undefined,
			},
		});
	}
}
