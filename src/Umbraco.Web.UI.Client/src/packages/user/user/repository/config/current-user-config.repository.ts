import type { UmbCurrentUserConfigurationModel } from '../../types.js';
import { UmbCurrentUserConfigServerDataSource } from './current-user-config.server.data-source.js';
import { UMB_CURRENT_USER_CONFIG_STORE_CONTEXT } from './current-user-config.store.token.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export class UmbCurrentUserConfigRepository extends UmbRepositoryBase implements UmbApi {
	/**
	 * Promise that resolves when the repository has been initialized, i.e. when the user configuration has been fetched from the server.
	 * @memberof UmbCurrentUserConfigRepository
	 */
	initialized: Promise<void>;

	#dataStore?: typeof UMB_CURRENT_USER_CONFIG_STORE_CONTEXT.TYPE;
	#dataSource = new UmbCurrentUserConfigServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.initialized = new Promise<void>((resolve) => {
			this.consumeContext(UMB_CURRENT_USER_CONFIG_STORE_CONTEXT, async (store) => {
				if (store) {
					this.#dataStore = store;
					await this.#init();
					resolve();
				}
			});
		});
	}

	async #init() {
		// Check if the store already has data
		if (this.#dataStore?.getState()) {
			return;
		}

		const { data } = await this.#dataSource.getCurrentUserConfig();

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
	 * @param part
	 */
	part<Part extends keyof UmbCurrentUserConfigurationModel>(
		part: Part,
	): Observable<UmbCurrentUserConfigurationModel[Part]> {
		if (!this.#dataStore) {
			throw new Error('Data store not initialized');
		}

		return this.#dataStore.part(part);
	}
}

export default UmbCurrentUserConfigRepository;
