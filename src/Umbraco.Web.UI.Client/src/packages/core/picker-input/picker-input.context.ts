import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { type UmbItemRepository, UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import {
	UMB_CONFIRM_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
	UmbModalToken,
	UmbPickerModalData,
	UmbPickerModalValue,
} from '@umbraco-cms/backoffice/modal';

export class UmbPickerInputContext<ItemType extends { name: string }> extends UmbBaseController {
	// TODO: We are way too unsecure about the requirements for the Modal Token, as we have certain expectation for the data and value.
	modalAlias: string | UmbModalToken<UmbPickerModalData<ItemType>, UmbPickerModalValue>;
	repository?: UmbItemRepository<ItemType>;
	#getUnique: (entry: ItemType) => string | undefined;

	public modalManager?: UmbModalManagerContext;

	#init: Promise<unknown>;

	#itemManager;

	selection;
	selectedItems;

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

	/* TODO: find a better way to have a getUniqueMethod. If we want to support trees/items of different types,
	then it need to be bound to the type and can't be a generic method we pass in. */
	constructor(
		host: UmbControllerHost,
		repositoryAlias: string,
		modalAlias: string | UmbModalToken<UmbPickerModalData<ItemType>, UmbPickerModalValue>,
		getUniqueMethod?: (entry: ItemType) => string | undefined,
	) {
		super(host);
		this.modalAlias = modalAlias;
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		//@ts-ignore
		this.#getUnique = getUniqueMethod || ((entry) => entry.id || '');

		this.#itemManager = new UmbRepositoryItemsManager<ItemType>(this, repositoryAlias, this.#getUnique);

		this.selection = this.#itemManager.uniques;
		this.selectedItems = this.#itemManager.items;

		this.#init = Promise.all([
			this.#itemManager.init,
			this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
				this.modalManager = instance;
			}).asPromise(),
		]);
	}

	getSelection() {
		return this.#itemManager.getUniques();
	}

	setSelection(selection: Array<string | null>) {
		// Note: Currently we do not support picking root item. So we filter out null values:
		this.#itemManager.setUniques(selection.filter((value) => value !== null) as Array<string>);
	}

	async openPicker(pickerData?: Partial<UmbPickerModalData<ItemType>>) {
		await this.#init;
		if (!this.modalManager) throw new Error('Modal manager context is not initialized');

		const modalContext = this.modalManager.open(this.modalAlias, {
			data: {
				multiple: this._max === 1 ? false : true,
				...pickerData,
			},
			value: {
				selection: this.getSelection(),
			},
		});

		const modalValue = await modalContext?.onSubmit();
		this.setSelection(modalValue.selection);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}

	async requestRemoveItem(unique: string) {
		// TODO: ID won't always be available on the model, so we need to get the unique property from somewhere. Maybe the repository?
		const item = this.#itemManager.getItems().find((item) => this.#getUnique(item) === unique);
		if (!item) throw new Error('Could not find item with unique: ' + unique);

		const modalContext = this.modalManager?.open(UMB_CONFIRM_MODAL, {
			data: {
				color: 'danger',
				headline: `Remove ${item.name}?`,
				content: 'Are you sure you want to remove this item',
				confirmLabel: 'Remove',
			},
		});

		await modalContext?.onSubmit();
		this.#removeItem(unique);
	}

	#removeItem(unique: string) {
		const newSelection = this.getSelection().filter((value) => value !== unique);
		this.setSelection(newSelection);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}
}
