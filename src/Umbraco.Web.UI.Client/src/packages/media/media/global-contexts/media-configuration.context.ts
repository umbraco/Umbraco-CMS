import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { MediaService, type MediaConfigurationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A context for fetching and caching the media configuration.
 * @internal
 */
export class UmbMediaConfigurationContext extends UmbContextBase implements UmbApi {
	/**
	 * The cached media configuration.
	 */
	static #mediaConfiguration: Promise<MediaConfigurationResponseModel | null>;

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_CONFIGURATION_CONTEXT);
	}

	/**
	 * Get the media configuration from the server, or return the cached configuration if it has already been fetched.
	 * @returns A promise that resolves to the media configuration, or null if the configuration could not be fetched.
	 */
	getMediaConfiguration(): Promise<MediaConfigurationResponseModel | null> {
		return (UmbMediaConfigurationContext.#mediaConfiguration ??= this.#fetchMediaConfiguration());
	}

	/**
	 * Fetch the media configuration from the server.
	 * @returns A promise that resolves to the media configuration, or null if the configuration could not be fetched.
	 */
	async #fetchMediaConfiguration() {
		const { data } = await tryExecute(this, MediaService.getMediaConfiguration());

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
