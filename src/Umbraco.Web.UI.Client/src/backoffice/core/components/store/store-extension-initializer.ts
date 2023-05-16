import { createExtensionClass, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export class UmbStoreExtensionInitializer {
	public host: UmbControllerHostElement;
	#storeMap = new Map();

	constructor(host: UmbControllerHostElement) {
		this.host = host;

		new UmbObserverController(
			this.host,
			umbExtensionsRegistry.extensionsOfTypes(['store', 'treeStore', 'itemStore']),
			(stores) => {
				if (!stores) return;

				stores.forEach((store) => {
					if (this.#storeMap.has(store.alias)) return;

					// Instantiate and provide stores. Stores are self providing when the class is instantiated.
					this.#storeMap.set(store.alias, createExtensionClass(store, [this.host]));
				});
			}
		);
	}
}
