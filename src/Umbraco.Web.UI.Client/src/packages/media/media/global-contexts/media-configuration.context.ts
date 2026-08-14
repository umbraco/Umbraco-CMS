import { UmbMediaConfigurationRepository } from '../configuration/index.js';
import type { UmbMediaConfigurationModel } from '../configuration/types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * A context for fetching and caching the media configuration.
 * @internal
 */
export class UmbMediaConfigurationContext extends UmbContextBase implements UmbApi {
	readonly #repository = new UmbMediaConfigurationRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_CONFIGURATION_CONTEXT);
	}

	/**
	 * Get the media configuration from the server, or return the cached configuration if it has already been fetched.
	 * @returns {Promise<UmbMediaConfigurationModel | null>} A promise that resolves to the media configuration, or null if the configuration could not be fetched.
	 */
	async getMediaConfiguration(): Promise<UmbMediaConfigurationModel | null> {
		const { data } = await this.#repository.requestConfiguration();

		return data ?? null;
	}
}

// Export as default to work as a global context:
export default UmbMediaConfigurationContext;

/**
 * @internal
 */
export const UMB_MEDIA_CONFIGURATION_CONTEXT = new UmbContextToken<UmbMediaConfigurationContext>(
	'UmbMediaConfigurationContext',
);
