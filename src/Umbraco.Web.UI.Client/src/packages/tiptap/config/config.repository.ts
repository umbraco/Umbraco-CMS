import type { UmbTiptapServerConfigurationModel } from './types.js';
import { UmbTiptapUmbracoPathConfigServerDataSource } from './config.server.data-source.js';
import { UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_CONTEXT } from './config.store.token.js';
import { UMB_TIPTAP_UMBRACO_PATH_REPOSITORY_ALIAS } from './constants.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export class UmbTiptapUmbracoPathConfigRepository extends UmbRepositoryBase implements UmbApi {
	/**
	 * Promise that resolves when the repository has been initialized, i.e. when the configuration has been fetched from the server.
	 */
	initialized;

	#dataStore?: typeof UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_CONTEXT.TYPE;
	#dataSource = new UmbTiptapUmbracoPathConfigServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_TIPTAP_UMBRACO_PATH_REPOSITORY_ALIAS.toString());
		this.initialized = new Promise<void>((resolve) => {
			this.consumeContext(UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_CONTEXT, async (store) => {
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

		const umbracoPathConfig = await this.requestUmbracoPathConfiguration();

		if (umbracoPathConfig) {
			this.#dataStore?.update(umbracoPathConfig);
		}
	}

	async requestUmbracoPathConfiguration() {
		const { data } = await this.#dataSource.getConfig();
		return data;
	}

	/**
	 * Subscribe to the entire configuration.
	 * @returns {Observable<UmbTiptapUmbracoPathConfigurationModel>}
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
	 * @returns {Observable<UmbTiptapServerConfigurationModel[Part]>}
	 */
	part<Part extends keyof UmbTiptapServerConfigurationModel>(
		part: Part,
	): Observable<UmbTiptapServerConfigurationModel[Part]> {
		if (!this.#dataStore) {
			throw new Error('Data store not initialized');
		}

		return this.#dataStore.part(part);
	}
}

export default UmbTiptapUmbracoPathConfigRepository;
