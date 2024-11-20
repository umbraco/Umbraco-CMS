import type { UmbStoreBase } from './store-base.js';
import { UmbStoreAppendEvent, UmbStoreRemoveEvent, UmbStoreUpdateEvent } from './events/index.js';
import type { UmbStore } from './store.interface.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbStoreConnectorArgs<StoreType, ConnectedStoreType> {
	store: UmbStoreBase;
	connectToStoreAlias: UmbContextToken<any, any> | string;
	newStoreItemMapper?: (item: ConnectedStoreType) => StoreType;
	updateStoreItemMapper?: (item: ConnectedStoreType) => Partial<StoreType>;
}

// WIP: get back to this when we have decided on detail model sizes
export class UmbStoreConnector<StoreType, ConnectedStoreType> {
	#store: UmbStore<StoreType>;
	#connectedStore?: UmbStore<ConnectedStoreType>;
	#newStoreItemMapper?: (item: ConnectedStoreType) => StoreType;
	#updateStoreItemMapper?: (item: ConnectedStoreType) => Partial<StoreType>;

	constructor(host: UmbControllerHost, args: UmbStoreConnectorArgs<StoreType, ConnectedStoreType>) {
		this.#store = args.store;
		this.#newStoreItemMapper = args.newStoreItemMapper;
		this.#updateStoreItemMapper = args.updateStoreItemMapper;

		new UmbContextConsumerController(host, args.connectToStoreAlias, (instance) => {
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
			if (storeHasItem && this.#updateStoreItemMapper) {
				this.#store.updateItem(unique, this.#updateStoreItemMapper(connectedStoreItem!));
			} else if (this.#newStoreItemMapper) {
				this.#store.append(this.#newStoreItemMapper(connectedStoreItem!));
			}
		});
	};

	#removeStoreItems = (event: UmbStoreRemoveEvent) => {
		this.#store.removeItems(event.uniques);
	};
}
