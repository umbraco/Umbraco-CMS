import { UmbContextConsumerController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreCreateEvent, UmbStoreDeleteEvent, UmbStoreUpdateEvent } from './events/index.js';
import { UmbStoreBase } from './store-base.js';

export class UmbStoreConnector {
	#store: UmbStoreBase;
	#connectedStore?: any;
	#mapperFunction: (item: any) => any;

	constructor(
		host: UmbControllerHostElement,
		store: UmbStoreBase,
		storeAlias: UmbContextToken<any, any> | string,
		mapperFunction: (item: any) => any,
	) {
		this.#store = store;
		this.#mapperFunction = mapperFunction;

		new UmbContextConsumerController(host, storeAlias, (instance) => {
			this.#connectedStore = instance;

			this.#connectedStore.events.addEventListener(
				UmbStoreCreateEvent.TYPE,
				this.#onConnectedStoreCreate as EventListener,
			);

			this.#connectedStore.events.addEventListener(
				UmbStoreUpdateEvent.TYPE,
				this.#onConnectedStoreUpdate as EventListener,
			);

			this.#connectedStore.events.addEventListener(
				UmbStoreDeleteEvent.TYPE,
				this.#onConnectedStoreDelete as EventListener,
			);
		});
	}

	#onConnectedStoreCreate = (event: UmbStoreCreateEvent) => {
		const items = this.#connectedStore!.getItems(event.uniques);
		const mappedItems = items.map((item) => this.#mapperFunction(item));
		this.#store.appendItems(mappedItems);
	};

	#onConnectedStoreUpdate = (event: UmbStoreUpdateEvent) => {
		const uniques = event.uniques;
		const items = this.#connectedStore!.getItems(uniques);
		const mappedItems = items.map((item) => this.#mapperFunction(item));
		mappedItems.forEach((mappedItem, index) => this.#store.updateItem(uniques[index], mappedItem));
	};

	#onConnectedStoreDelete = (event: UmbStoreDeleteEvent) => {
		this.#store.removeItems(event.uniques);
	};
}
