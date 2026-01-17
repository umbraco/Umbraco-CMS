import { UMB_PICKER_INPUT_CONTEXT } from './picker-input.context-token.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
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
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbModalRouteRegistrationController, type UmbModalRouteSetupReturn } from '@umbraco-cms/backoffice/router';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbPickerInputContext<
	PickedItemType extends UmbItemModel = UmbItemModel,
	PickerItemType extends UmbItemModel = UmbItemModel,
	PickerModalConfigType extends UmbPickerModalData<PickerItemType> = UmbPickerModalData<PickerItemType>,
	PickerModalValueType extends UmbPickerModalValue = UmbPickerModalValue,
> extends UmbContextBase {
	modalAlias?: string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType>;
	repository?: UmbItemRepository<PickedItemType>;

	#itemManager;

	public readonly selection;
	public readonly selectedItems;
	public readonly statuses;
	public readonly interactionMemory = new UmbInteractionMemoryManager(this);

	#modalRoute = new UmbStringState<string | undefined>(undefined);
	public readonly modalRoute = this.#modalRoute.asObservable();

	#modalData?: Partial<PickerModalConfigType>;

	/**
	 * Define a maximum amount of selected items in this input, for this input to be valid.
	 * @returns {number} The maximum number of items required.
	 */
	public get max() {
		return this._max;
	}
	public set max(value) {
		this._max = value === undefined ? Infinity : value;
	}
	private _max = Infinity;

	/**
	 * Define a minimum amount of selected items in this input, for this input to be valid.
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
	 * Creates an instance of UmbPickerInputContext.
	 * @param {UmbControllerHost} host - The host for the controller.
	 * @param {string} repositoryAlias - The alias of the repository to use.
	 * @param {(string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType>)} modalAlias - The alias of the modal to use.
	 * @memberof UmbPickerInputContext
	 */
	constructor(
		host: UmbControllerHost,
		repositoryAlias: string,
		modalAlias?: string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType>,
	) {
		super(host, UMB_PICKER_INPUT_CONTEXT);

		if (modalAlias) {
			this.setModalAlias(modalAlias);
		}

		this.#itemManager = new UmbRepositoryItemsManager<PickedItemType>(this, repositoryAlias);

		this.selection = this.#itemManager.uniques;
		this.statuses = this.#itemManager.statuses;
		this.selectedItems = this.#itemManager.items;
	}

	getSelection() {
		return this.#itemManager.getUniques();
	}
	getSelectedItems() {
		return this.#itemManager.getItems();
	}
	getSelectedItemByUnique(unique: string) {
		return this.#itemManager.getItems().find((item) => item.unique === unique);
	}

	setSelection(selection: Array<string | null>) {
		// Note: Currently we do not support picking root item. So we filter out null values:
		this.#itemManager.setUniques(selection.filter((value) => value !== null) as Array<string>);
	}

	/**
	 * Sets the modal alias/token to use for the picker modal.
	 * @param {string | UmbModalToken} modalAlias The modal alias or token.
	 * @memberof UmbPickerInputContext
	 */
	setModalAlias(modalAlias: string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType>) {
		this.modalAlias = modalAlias;
		this.#createPickerModalRoute();
	}

	/**
	 * Gets the modal alias/token used for the picker modal.
	 * @returns {string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType>} The modal alias or token.
	 * @memberof UmbPickerInputContext
	 */
	getModalAlias(): string | UmbModalToken<UmbPickerModalData<PickerItemType>, PickerModalValueType> | undefined {
		return this.modalAlias;
	}

	/**
	 * Sets modal data that will be used as base configuration for both direct openPicker() calls and modal route setup.
	 * @param {Partial<PickerModalConfigType>} modalData The modal data to store.
	 * @memberof UmbPickerInputContext
	 */
	setModalData(modalData?: Partial<PickerModalConfigType>) {
		this.#modalData = modalData;
	}

	/**
	 * Gets the stored modal data.
	 * @returns {Partial<PickerModalConfigType> | undefined} The stored modal data.
	 * @memberof UmbPickerInputContext
	 */
	getModalData(): Partial<PickerModalConfigType> | undefined {
		return this.#modalData;
	}

	async openPicker(pickerData?: Partial<PickerModalConfigType>) {
		await this.#itemManager.init;

		if (!this.modalAlias) {
			throw new Error('No modal alias defined for the picker input context.');
		}

		const modalValue = await umbOpenModal(this, this.modalAlias, {
			data: this.#getPickerModalDataArgs(pickerData),
			value: this.#getPickerModalValueArgs(),
		}).catch(() => undefined);

		this.#applyModalValue(modalValue);
	}

	protected async _requestItemName(unique: string) {
		return this.getSelectedItemByUnique(unique)?.name ?? '#general_notFound';
	}

	async requestRemoveItem(unique: string) {
		const name = await this._requestItemName(unique);
		await umbConfirmModal(this, {
			color: 'danger',
			headline: `#actions_remove?`,
			content: `#defaultdialogs_confirmremove ${name}?`,
			confirmLabel: '#actions_remove',
		});

		this._removeItem(unique);
	}

	protected _removeItem(unique: string) {
		const newSelection = this.getSelection().filter((value) => value !== unique);
		this.setSelection(newSelection);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}

	#pickerModalRouteRegistration?: UmbModalRouteRegistrationController<
		UmbPickerModalData<PickerItemType>,
		PickerModalValueType
	>;

	#createPickerModalRoute() {
		if (!this.modalAlias) {
			this.#pickerModalRouteRegistration?.destroy();
			return;
		}

		this.#pickerModalRouteRegistration = new UmbModalRouteRegistrationController(this, this.modalAlias)
			.addUniquePaths(['picker'])
			.onSetup(() => {
				return {
					data: this.#getPickerModalDataArgs(),
					value: this.#getPickerModalValueArgs(),
				} as UmbModalRouteSetupReturn<UmbPickerModalData<PickerItemType>, PickerModalValueType>;
			})
			.onSubmit((value) => {
				this.#applyModalValue(value);
			})
			.observeRouteBuilder((routeBuilder) => {
				const path = routeBuilder({});
				this.#modalRoute.setValue(path);
			});
	}

	#getPickerModalDataArgs(modalData?: Partial<PickerModalConfigType>) {
		return {
			multiple: this._max === 1 ? false : true,
			...this.#modalData,
			...modalData,
		};
	}

	#getPickerModalValueArgs(): PickerModalValueType {
		return {
			selection: this.getSelection(),
		} as PickerModalValueType;
	}

	#applyModalValue(value: PickerModalValueType | undefined) {
		if (!value) return;
		this.setSelection(value.selection);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}
}
