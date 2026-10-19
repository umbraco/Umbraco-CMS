import type { UmbElementConfigurationModel } from './types.js';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

type UmbElementConfigurationResponse = UmbRepositoryResponse<UmbElementConfigurationModel>;

let configurationPromise: Promise<UmbElementConfigurationResponse> | undefined;

/**
 * Returns the cached element configuration, or requests it if it has not been fetched yet.
 * Concurrent calls share one request and error responses are not cached.
 * @param {() => Promise<UmbElementConfigurationResponse>} request - Fetches the configuration when nothing is cached.
 * @returns {Promise<UmbElementConfigurationResponse>} - The element configuration.
 * @internal
 */
export async function _getCachedElementConfiguration(
	request: () => Promise<UmbElementConfigurationResponse>,
): Promise<UmbElementConfigurationResponse> {
	configurationPromise ??= request();
	const response = await configurationPromise;
	if (response.error) {
		configurationPromise = undefined;
	}
	return response;
}

/**
 * Clears the cached element configuration.
 * @internal
 */
export function _resetElementConfigurationCacheForTesting(): void {
	configurationPromise = undefined;
}
