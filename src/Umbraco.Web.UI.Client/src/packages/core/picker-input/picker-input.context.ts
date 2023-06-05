import { UmbItemRepository, UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_CONFIRM_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
	UmbModalToken,
	UmbPickerModalData,
} from '@umbraco-cms/backoffice/modal';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';

export class UmbPickerInputContext<ItemType extends ItemResponseModelBaseModel> {
	host: UmbControllerHostElement;
	modalAlias: string | UmbModalToken;
	repository?: UmbItemRepository<ItemType>;
	#getUnique: (entry: ItemType) => string | undefined;

	public modalManager?: UmbModalManagerContext;

	public pickableFilter?: (item: ItemType) => boolean = () => true;

	#init: Promise<unknown>;

	#itemManager;

	selection;
	selectedItems;

	max = Infinity;
	min = 0;

	/* TODO: find a better way to have a getUniqueMethod. If we want to support trees/items of different types,
	then it need to be bound to the type and can't be a generic method we pass in. */
	constructor(
		host: UmbControllerHostElement,
		repositoryAlias: string,
		modalAlias: string | UmbModalToken,
		getUniqueMethod?: (entry: ItemType) => string | undefined
	) {
		this.host = host;
		this.modalAlias = modalAlias;
		this.#getUnique = getUniqueMethod || ((entry) => entry.id || '');

		this.#itemManager = new UmbRepositoryItemsManager<ItemType>(host, repositoryAlias, this.#getUnique);

		this.selection = this.#itemManager.uniques;
		this.selectedItems = this.#itemManager.items;

		this.#init = Promise.all([
			this.#itemManager.init,
			new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
				this.modalManager = instance;
			}).asPromise(),
		]);
	}

	getSelection() {
		return this.#itemManager.getUniques();
	}

	setSelection(selection: string[]) {
		this.#itemManager.setUniques(selection);
	}

	// TODO: If modalAlias is a ModalToken, then via TS, we should get the correct type for pickerData. Otherwise fallback to unknown.
	openPicker(pickerData?: Partial<UmbPickerModalData<ItemType>>) {
		if (!this.modalManager) throw new Error('Modal manager context is not initialized');

		const modalHandler = this.modalManager.open(this.modalAlias, {
			multiple: this.max === 1 ? false : true,
			selection: [...this.getSelection()],
			pickableFilter: this.pickableFilter,
			...pickerData,
		});

		modalHandler?.onSubmit().then(({ selection }: any) => {
			this.setSelection(selection);
			this.host.dispatchEvent(new UmbChangeEvent());
			// TODO: we only want to request items that are not already in the selectedItems array
		});
	}

	async requestRemoveItem(unique: string) {
		await this.#init;
		if (!this.repository) throw new Error('Repository is not initialized');

		// TODO: id won't always be available on the model, so we need to get the unique property from somewhere. Maybe the repository?
		const item = this.#itemManager.getItems().find((item) => this.#getUnique(item) === unique);
		if (!item) throw new Error('Could not find item with unique: ' + unique);

		const modalHandler = this.modalManager?.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Remove ${item.name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		await modalHandler?.onSubmit();
		this.#removeItem(unique);
	}

	#removeItem(unique: string) {
		const newSelection = this.getSelection().filter((value) => value !== unique);
		this.setSelection(newSelection);
	}
}
