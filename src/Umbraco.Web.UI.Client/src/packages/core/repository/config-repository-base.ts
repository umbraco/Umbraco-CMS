import { UmbRepositoryBase } from './repository-base.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbStoreObjectBase } from '@umbraco-cms/backoffice/store';
import { type Observable, from, switchMap } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * Base class for a repository exposing a single configuration object backed by an {@link UmbStoreObjectBase}.
 * The configuration is fetched once per store context on initialization; `all()` and `part()` defer internally
 * until it is ready, so consumers can subscribe immediately without awaiting.
 */
export abstract class UmbConfigRepositoryBase<ConfigModel> extends UmbRepositoryBase implements UmbApi {
	#initialized: Promise<void>;
	#dataStore?: UmbStoreObjectBase<ConfigModel>;

	/**
	 * Promise that resolves when the repository has been initialized, i.e. when the configuration has been fetched from the server.
	 * @deprecated Deprecated since v17. Awaiting this is no longer necessary — all() and part() defer internally
	 * until the configuration is ready, so you can subscribe directly. Scheduled for removal in Umbraco 19.
	 * @returns {Promise<void>} A promise that resolves once the configuration has been fetched.
	 */
	get initialized(): Promise<void> {
		new UmbDeprecation({
			deprecated: 'The "initialized" property on config repositories is deprecated.',
			removeInVersion: '19.0.0',
			solution:
				'Awaiting initialized is no longer necessary — all() and part() defer internally until the configuration is ready. Subscribe directly.',
		}).warn();
		return this.#initialized;
	}

	constructor(
		host: UmbControllerHost,
		storeContext: UmbContextToken<UmbStoreObjectBase<ConfigModel>>,
		repositoryAlias?: string,
	) {
		super(host, repositoryAlias);
		this.#initialized = new Promise<void>((resolve) => {
			this.consumeContext(storeContext, async (store) => {
				if (store) {
					this.#dataStore = store;
					await this.#init();
					resolve();
				}
			});
		});
	}

	/**
	 * Fetches the configuration from the server. Implemented by the concrete repository using its own data source.
	 * @returns {Promise<ConfigModel | undefined>} The configuration, or undefined if it could not be fetched.
	 */
	protected abstract _requestConfig(): Promise<ConfigModel | undefined>;

	async #init() {
		// Configuration is immutable per session — only fetch once per store context.
		if (this.#dataStore?.getState()) {
			return;
		}
		const config = await this._requestConfig();
		if (config) {
			this.#dataStore?.update(config);
		}
	}

	/**
	 * Subscribe to the entire configuration.
	 * @returns {Observable<ConfigModel | null>} An observable of the configuration.
	 */
	all(): Observable<ConfigModel | null> {
		return from(this.#initialized).pipe(switchMap(() => this.#dataStore!.all()));
	}

	/**
	 * Subscribe to a part of the configuration.
	 * @param {Part} part The configuration key to subscribe to.
	 * @returns {Observable<ConfigModel[Part]>} An observable of that part.
	 */
	part<Part extends keyof ConfigModel>(part: Part): Observable<ConfigModel[Part]> {
		return from(this.#initialized).pipe(switchMap(() => this.#dataStore!.part(part)));
	}
}
