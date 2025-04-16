import type { UmbTemporaryFileConfigurationModel } from '../types.js';
import { UmbTemporaryFileConfigServerDataSource } from './config.server.data-source.js';
import { UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT } from './config.store.token.js';
import { UMB_TEMPORARY_FILE_REPOSITORY_ALIAS } from './constants.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export class UmbTemporaryFileConfigRepository extends UmbRepositoryBase implements UmbApi {
	/**
	 * Promise that resolves when the repository has been initialized, i.e. when the configuration has been fetched from the server.
	 */
	initialized;

	#dataStore?: typeof UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT.TYPE;
	#dataSource = new UmbTemporaryFileConfigServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPORARY_FILE_REPOSITORY_ALIAS.toString());
		this.initialized = new Promise<void>((resolve) => {
			this.consumeContext(UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT, async (store) => {
				this.#dataStore = store;
				await this.#init();
				resolve();
			});
		});
	}

	async #init() {
		// Check if the store already has data
		if (this.#dataStore?.getState()) {
			return;
		}

		const { data } = await this.#dataSource.getConfig();

		if (data) {
			this.#dataStore?.update(data);
		}
	}

	/**
	 * Subscribe to the entire configuration.
	 */
	all() {
		if (!this.#dataStore) {
			throw new Error('Data store not initialized');
		}

		return this.#dataStore.all();
	}

	/**
	 * Subscribe to a part of the configuration.
	 * @param part
	 */
	part<Part extends keyof UmbTemporaryFileConfigurationModel>(
		part: Part,
	): Observable<UmbTemporaryFileConfigurationModel[Part]> {
		if (!this.#dataStore) {
			throw new Error('Data store not initialized');
		}

		return this.#dataStore.part(part);
	}
}

export default UmbTemporaryFileConfigRepository;
