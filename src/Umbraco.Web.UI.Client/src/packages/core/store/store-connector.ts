import { UmbStoreBase } from './store-base.js';
import { UmbStoreCreateEvent, UmbStoreDeleteEvent, UmbStoreUpdateEvent } from './events/index.js';
import { UmbStore } from './store.interface.js';
import { UmbContextConsumerController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStoreConnector {
	#store: UmbStoreBase;
	#connectedStore?: UmbStore<any>;
	#mapperFunction: (item: any) => any;

	constructor(
		host: UmbControllerHost,
		store: UmbStoreBase,
		connectToStoreAlias: UmbContextToken<any, any> | string,
		mapperFunction: (item: any) => any,
	) {
		this.#store = store;
		this.#mapperFunction = mapperFunction;

		new UmbContextConsumerController(host, connectToStoreAlias, (instance) => {
			this.#connectedStore = instance;

			if (this.#connectedStore) {
				this.#listenToConnectedStore();
			}
		});
	}

	#listenToConnectedStore = () => {
		if (!this.#connectedStore) return;
		this.#connectedStore.addEventListener(UmbStoreCreateEvent.TYPE, this.#onConnectedStoreCreate as EventListener);
		this.#connectedStore.addEventListener(UmbStoreUpdateEvent.TYPE, this.#onConnectedStoreUpdate as EventListener);
		this.#connectedStore.addEventListener(UmbStoreDeleteEvent.TYPE, this.#onConnectedStoreDelete as EventListener);
	};

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
