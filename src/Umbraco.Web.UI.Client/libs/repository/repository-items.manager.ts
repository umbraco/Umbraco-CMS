import { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { createExtensionClass } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbRepositoryItemsManager<ItemType extends ItemResponseModelBaseModel> {
	host: UmbControllerHostElement;
	repository?: UmbItemRepository<ItemType>;
	#getUnique: (entry: ItemType) => string | undefined;

	init: Promise<void>;

	#uniques = new UmbArrayState<string>([]);
	uniques = this.#uniques.asObservable();

	#items = new UmbArrayState<ItemType>([]);
	items = this.#items.asObservable();

	itemsObserver?: UmbObserverController<ItemType[]>;

	/* TODO: find a better way to have a getUniqueMethod. If we want to support trees/items of different types,
	then it need to be bound to the type and can't be a generic method we pass in. */
	constructor(
		host: UmbControllerHostElement,
		repositoryAlias: string,
		getUniqueMethod?: (entry: ItemType) => string | undefined
	) {
		this.host = host;
		this.#getUnique = getUniqueMethod || ((entry) => entry.id || '');

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

	getUniques() {
		return this.#uniques.value;
	}

	setUniques(uniques: string[]) {
		this.#uniques.next(uniques);

		//TODO: Check if it's safe to call requestItems here.
		this.#requestItems();
	}

	getItems() {
		return this.#items.value;
	}

	async #requestItems() {
		await this.init;
		if (!this.repository) throw new Error('Repository is not initialized');
		if (this.itemsObserver) this.itemsObserver.destroy();

		// TODO: Test if its just some items that is gone now, if so then just filter them out. (maybe use code from #removeItem)

		const { asObservable } = await this.repository.requestItems(this.getUniques());

		if (asObservable) {
			this.itemsObserver = new UmbObserverController(this.host, asObservable(), (data) => this.#items.next(data));
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
