import { UmbStoreBase } from './store-base.js';
import { UmbStoreAppendEvent, UmbStoreRemoveEvent, UmbStoreUpdateEvent } from './events/index.js';
import { UmbStore } from './store.interface.js';
import { UmbContextConsumerController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// WIP: get back to this when we have decided on detail model sizes
export class UmbStoreConnector<StoreType, ConnectedStoreType> {
	#store: UmbStore<StoreType>;
	#connectedStore?: UmbStore<ConnectedStoreType>;
	#onNewStoreItem: (item: ConnectedStoreType) => StoreType;
	#onUpdateStoreItem: (item: ConnectedStoreType) => StoreType;

	constructor(
		host: UmbControllerHost,
		store: UmbStoreBase,
		connectToStoreAlias: UmbContextToken<any, any> | string,
		onNewStoreItem: (item: ConnectedStoreType) => StoreType,
		onUpdateStoreItem: (item: ConnectedStoreType) => Partial<StoreType>,
	) {
		this.#store = store;
		this.#onNewStoreItem = onNewStoreItem;
		this.#onUpdateStoreItem = onUpdateStoreItem;

		console.warn('UmbStoreConnector is a work in progress and should not be used yet');

		new UmbContextConsumerController(host, connectToStoreAlias, (instance) => {
			this.#connectedStore = instance;

			if (this.#connectedStore) {
				this.#listenToConnectedStore();
			}
		});
	}

	#listenToConnectedStore = () => {
		if (!this.#connectedStore) return;
		this.#connectedStore.addEventListener(UmbStoreAppendEvent.TYPE, this.#updateStoreItems as EventListener);
		this.#connectedStore.addEventListener(UmbStoreUpdateEvent.TYPE, this.#updateStoreItems as EventListener);
		this.#connectedStore.addEventListener(UmbStoreRemoveEvent.TYPE, this.#removeStoreItems as EventListener);
	};

	#updateStoreItems = (event: UmbStoreAppendEvent | UmbStoreUpdateEvent) => {
		event.uniques.forEach((unique) => {
			const storeHasItem = this.#store.getItems([unique]).length > 0;
			const connectedStoreItem = this.#connectedStore!.getItems([unique])[0];

			// we check if the current store has the item from the connected store, if it does we update it, if it doesn't we append it
			if (storeHasItem) {
				this.#store.updateItem(unique, this.#onUpdateStoreItem(connectedStoreItem!));
			} else {
				this.#store.append(this.#onNewStoreItem(connectedStoreItem!));
			}
		});
	};

	#removeStoreItems = (event: UmbStoreRemoveEvent) => {
		this.#store.removeItems(event.uniques);
	};
}
