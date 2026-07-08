import { UmbElementConfigurationRepository } from '../configuration/configuration.repository.js';
import type { UmbElementConfigurationModel } from '../configuration/types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * A context for fetching and caching the element configuration.
 * @internal
 */
export class UmbElementConfigurationContext extends UmbContextBase implements UmbApi {
	readonly #repository = new UmbElementConfigurationRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_ELEMENT_CONFIGURATION_CONTEXT);
	}

	/**
	 * Get the element configuration from the server, or return the cached configuration if it has already been fetched.
	 * @returns {Promise<UmbElementConfigurationModel | null>} A promise that resolves to the element configuration, or null if the configuration could not be fetched.
	 */
	async getElementConfiguration(): Promise<UmbElementConfigurationModel | null> {
		const { data } = await this.#repository.requestConfiguration();
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
