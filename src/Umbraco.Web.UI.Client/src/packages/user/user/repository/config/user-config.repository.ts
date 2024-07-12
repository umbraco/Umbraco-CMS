import type { UmbUserConfigurationModel } from '../../types.js';
import { UmbUserConfigServerDataSource } from './user-config.server.data-source.js';
import { UMB_USER_CONFIG_STORE_CONTEXT } from './user-config.store.token.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserConfigRepository extends UmbRepositoryBase implements UmbApi {
	#dataStore?: typeof UMB_USER_CONFIG_STORE_CONTEXT.TYPE;
	#dataSource = new UmbUserConfigServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.consumeContext(UMB_USER_CONFIG_STORE_CONTEXT, (store) => {
			this.#dataStore = store;
			this.#init();
		});
	}

	async #init() {
		// Check if the store already has data
		if (this.#dataStore?.getState()) {
			return;
		}

		const { data } = await this.#dataSource.getUserConfig();

		if (data) {
			this.#dataStore?.update(data);
		}
	}

	/**
	 * Subscribe to the entire user configuration.
	 */
	all() {
		if (!this.#dataStore) {
			throw new Error('Data store not initialized');
		}

		return this.#dataStore.all();
	}

	/**
	 * Subscribe to a part of the user configuration.
	 */
	part<Part extends keyof UmbUserConfigurationModel>(part: Part): Observable<UmbUserConfigurationModel[Part]> {
		if (!this.#dataStore) {
			throw new Error('Data store not initialized');
		}

		return this.#dataStore.part(part);
	}
}

export default UmbUserConfigRepository;
