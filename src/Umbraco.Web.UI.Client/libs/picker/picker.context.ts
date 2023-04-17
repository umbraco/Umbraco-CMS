import { UmbTreeRepository } from '../repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ArrayState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { createExtensionClass, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import {
	UMB_CONFIRM_MODAL,
	UMB_MODAL_CONTEXT_TOKEN,
	UmbModalContext,
	UmbModalToken,
} from '@umbraco-cms/backoffice/modal';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbPickerContext<ItemType extends ItemResponseModelBaseModel> {
	host: UmbControllerHostElement;
	modalAlias: UmbModalToken | string;
	repository?: UmbTreeRepository<ItemType>;

	public modalContext?: UmbModalContext;

	#selection = new ArrayState<string>([]);
	selection = this.#selection.asObservable();

	#selectedItems = new ArrayState<ItemType>([]);
	selectedItems = this.#selectedItems.asObservable();

	#selectedItemsObserver?: UmbObserverController<ItemType[]>;

	max = Infinity;
	min = 0;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, modalAlias: UmbModalToken | string) {
		this.host = host;
		this.modalAlias = modalAlias;

		// TODO: unsure a method can't be called before everything is initialized
		new UmbObserverController(
			this.host,

			// TODO: this code is reused in multiple places, so it should be extracted to a function
			umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionClass<UmbTreeRepository>(repositoryManifest, [this.host]);
					this.repository = result;
				} catch (error) {
					throw new Error('Could not create repository with alias: ' + repositoryAlias + '');
				}
			}
		);

		new UmbContextConsumerController(this.host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.modalContext = instance;
		});
	}

	getSelection() {
		return this.#selection.value;
	}

	setSelection(selection: string[]) {
		this.#selection.next(selection);
	}

	// TODO: this need to accept an options object at some point to pass to the modal context
	openPicker() {
		if (!this.modalContext) throw new Error('Modal context is not initialized');

		const modalHandler = this.modalContext.open(this.modalAlias, {
			multiple: this.max === 1 ? false : true,
			selection: [...this.getSelection()],
		});

		modalHandler?.onSubmit().then(({ selection }: any) => {
			this.setSelection(selection);
			// TODO: we only want to request items that are not already in the selectedItems array
			this.#requestItems();
		});
	}

	async requestRemoveItem(unique: string) {
		if (!this.repository) throw new Error('Repository is not initialized');

		const { data } = await this.repository.requestTreeItems([unique]);
		if (!data) throw new Error('Could not find item with unique id: ' + unique);

		const modalHandler = this.modalContext?.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Remove ${data[0].name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		await modalHandler?.onSubmit();
		this.#removeItem(unique);
	}

	async #requestItems() {
		if (!this.repository) throw new Error('Repository is not initialized');
		if (this.#selectedItemsObserver) this.#selectedItemsObserver.destroy();

		const { asObservable } = await this.repository.requestTreeItems(this.getSelection());

		if (asObservable) {
			this.#selectedItemsObserver = new UmbObserverController(this.host, asObservable(), (data) => {
				this.#selectedItems.next(data);
			});
		}
	}

	#removeItem(unique: string) {
		const newSelection = this.getSelection().filter((value) => value !== unique);
		this.setSelection(newSelection);

		// remove items items from selectedItems array
		// TODO: id won't always be available on the model, so we need to get the unique property from somewhere. Maybe the repository?
		const newSelectedItems = this.#selectedItems.value.filter((item) => item.id !== unique);
		this.#selectedItems.next(newSelectedItems);
	}
}
