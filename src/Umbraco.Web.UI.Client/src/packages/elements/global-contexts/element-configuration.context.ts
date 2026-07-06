import { UmbElementConfigurationRepository } from '../configuration/configuration.repository.js';
import type { UmbElementConfigurationModel } from '../configuration/types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

// TODO: Turn this into a Repository with a Store that holds the cache [NL]
/**
 * A context for fetching and caching the element configuration.
 * @internal
 */
export class UmbElementConfigurationContext extends UmbContextBase implements UmbApi {
	/**
	 * The cached element configuration.
	 */
	static #elementConfiguration: Promise<UmbElementConfigurationModel | null>;

	constructor(host: UmbControllerHost) {
		super(host, UMB_ELEMENT_CONFIGURATION_CONTEXT);
	}

	/**
	 * Get the element configuration from the server, or return the cached configuration if it has already been fetched.
	 * @returns A promise that resolves to the element configuration, or null if the configuration could not be fetched.
	 */
	getElementConfiguration(): Promise<UmbElementConfigurationModel | null> {
		return (UmbElementConfigurationContext.#elementConfiguration ??= this.fetchElementConfiguration());
	}

	/**
	 * Fetch the element configuration from the server.
	 * @returns A promise that resolves to the element configuration, or null if the configuration could not be fetched.
	 */
	async fetchElementConfiguration() {
		const { data } = await new UmbElementConfigurationRepository(this).requestConfiguration();

		return data ?? null;
	}
}

// Export as default to work as a global context:
export default UmbElementConfigurationContext;

/**
 * @internal
 */
export const UMB_ELEMENT_CONFIGURATION_CONTEXT = new UmbContextToken<UmbElementConfigurationContext>(
	'UmbElementConfigurationContext',
);
