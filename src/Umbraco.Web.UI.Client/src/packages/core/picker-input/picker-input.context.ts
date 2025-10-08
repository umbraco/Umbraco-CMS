import { UMB_PICKER_INPUT_CONTEXT } from './picker-input.context-token.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import {
	umbConfirmModal,
	umbOpenModal,
	type UmbModalToken,
	type UmbPickerModalData,
	type UmbPickerModalValue,
} from '@umbraco-cms/backoffice/modal';

type PickerItemBaseType = { name: string; unique: string };
export class UmbPickerInputContext<
	PickedItemType extends PickerItemBaseType = PickerItemBaseType,
	PickerItemType extends PickerItemBaseType = PickedItemType,
	PickerModalConfigType extends UmbPickerModalData<PickerItemType> = UmbPickerModalData<PickerItemType>,
	PickerModalValueType extends UmbPickerModalValue = UmbPickerModalValue,
> extends UmbContextBase {
	modalAlias: string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType>;
	repository?: UmbItemRepository<PickedItemType>;
	#getUnique: (entry: PickedItemType) => string | undefined;

	#itemManager;

	public readonly selection;
	public readonly selectedItems;
	public readonly statuses;
	public readonly interactionMemory = new UmbInteractionMemoryManager(this);

	/**
	 * Define a minimum amount of selected items in this input, for this input to be valid.
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
	 */
	public get min() {
		return this._min;
	}
	public set min(value) {
		this._min = value === undefined ? 0 : value;
	}
	private _min = 0;

	/**
	 * Creates an instance of UmbPickerInputContext.
	 * @param {UmbControllerHost} host - The host for the controller.
	 * @param {string} repositoryAlias - The alias of the repository to use.
	 * @param {(string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType>)} modalAlias - The alias of the modal to use.
	 * @param {((entry: PickedItemType) => string | undefined)} [getUniqueMethod] - DEPRECATED since 15.3. Will be removed in v. 17: A method to get the unique key from the item.
	 * @memberof UmbPickerInputContext
	 */
	constructor(
		host: UmbControllerHost,
		repositoryAlias: string,
		modalAlias: string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType>,
		getUniqueMethod?: (entry: PickedItemType) => string | undefined,
	) {
		super(host, UMB_PICKER_INPUT_CONTEXT);
		this.modalAlias = modalAlias;

		this.#getUnique = getUniqueMethod
			? (entry: PickedItemType) => {
					new UmbDeprecation({
						deprecated: 'The getUniqueMethod parameter.',
						removeInVersion: '17.0.0',
						solution: 'The required unique property on the item will be used instead.',
					}).warn();
					return getUniqueMethod(entry);
				}
			: (entry) => entry.unique;

		this.#itemManager = new UmbRepositoryItemsManager<PickedItemType>(this, repositoryAlias, getUniqueMethod);

		this.selection = this.#itemManager.uniques;
		this.statuses = this.#itemManager.statuses;
		this.selectedItems = this.#itemManager.items;
	}

	getSelection() {
		return this.#itemManager.getUniques();
	}

	setSelection(selection: Array<string | null>) {
		// Note: Currently we do not support picking root item. So we filter out null values:
		this.#itemManager.setUniques(selection.filter((value) => value !== null) as Array<string>);
	}

	async openPicker(pickerData?: Partial<PickerModalConfigType>) {
		await this.#itemManager.init;
		const modalValue = await umbOpenModal(this, this.modalAlias, {
			data: {
				multiple: this._max === 1 ? false : true,
				...pickerData,
			},
			value: {
				selection: this.getSelection(),
			} as PickerModalValueType,
		}).catch(() => undefined);

		if (!modalValue) return;

		this.setSelection(modalValue.selection);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}

	async requestRemoveItem(unique: string) {
		const item = this.#itemManager.getItems().find((item) => this.#getUnique(item) === unique);

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
		this.#itemManager.removeStatus(unique);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}
}
