import { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { createExtensionClass } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbRepositorySelectionManager<ItemType extends ItemResponseModelBaseModel> {
	host: UmbControllerHostElement;
	repository?: UmbItemRepository<ItemType>;

	init: Promise<void>;

	#selection = new UmbArrayState<string>([]);
	selection = this.#selection.asObservable();

	#selectedItems = new UmbArrayState<ItemType>([]);
	selectedItems = this.#selectedItems.asObservable();

	#selectedItemsObserver?: UmbObserverController<ItemType[]>;

	/* TODO: find a better way to have a getUniqueMethod. If we want to support trees/items of different types,
	then it need to be bound to the type and can't be a generic method we pass in. */
	constructor(host: UmbControllerHostElement, repositoryAlias: string) {
		this.host = host;

		//TODO: The promise can probably be done in a cleaner way.
		this.init = new Promise((resolve) => {
			new UmbObserverController(
				this.host,

				// TODO: this code is reused in multiple places, so it should be extracted to a function
				umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
				async (repositoryManifest) => {
					if (!repositoryManifest) return;

					try {
						const result = await createExtensionClass<UmbItemRepository<ItemType>>(repositoryManifest, [this.host]);
						this.repository = result;
						resolve();
					} catch (error) {
						throw new Error('Could not create repository with alias: ' + repositoryAlias + '');
					}
				}
			);
		});
	}

	getSelection() {
		return this.#selection.value;
	}

	setSelection(selection: string[]) {
		this.#selection.next(selection);

		//TODO: Check if it's safe to call requestItems here.
		this.#requestItems();
	}

	getSelectedItems() {
		return this.#selectedItems.value;
	}

	async #requestItems() {
		await this.init;
		if (!this.repository) throw new Error('Repository is not initialized');
		if (this.#selectedItemsObserver) this.#selectedItemsObserver.destroy();

		// TODO: Test if its just some items that is gone now, if so then just filter them out. (maybe use code from #removeItem)

		const { asObservable } = await this.repository.requestItems(this.getSelection());

		if (asObservable) {
			this.#selectedItemsObserver = new UmbObserverController(this.host, asObservable(), (data) =>
				this.#selectedItems.next(data)
			);
		}
	}

	/*
	#removeItem(unique: string) {
		const newSelection = this.getSelection().filter((value) => value !== unique);
		this.#selection.next(newSelection);
		// remove items items from selectedItems array
		// TODO: id won't always be available on the model, so we need to get the unique property from somewhere. Maybe the repository?
		const newSelectedItems = this.#selectedItems.value.filter((item) => this.#getUnique(item) !== unique);
		this.#selectedItems.next(newSelectedItems);
	}
	*/
}
