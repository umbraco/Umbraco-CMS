import { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbExtensionClassInitializer } from '@umbraco-cms/backoffice/extension-api';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbRepositoryItemsManager<ItemType extends ItemResponseModelBaseModel> {
	host: UmbControllerHostElement;
	repository?: UmbItemRepository<ItemType>;
	#getUnique: (entry: ItemType) => string | undefined;

	init: Promise<unknown>;

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

		this.init = new UmbExtensionClassInitializer(host, 'repository', repositoryAlias, (repository) => {
			// TODO: Some test that this repository is a items repository?
			this.repository = repository as UmbItemRepository<ItemType>;
		}).asPromise();
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
		// This is where this.#getUnique comes in play. Unless that can come from the repository, but that collides with the idea of having a multi-type repository. If that happens.

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
