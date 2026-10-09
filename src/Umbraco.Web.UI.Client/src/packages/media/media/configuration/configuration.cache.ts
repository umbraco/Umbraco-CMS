import type { UmbMediaConfigurationModel } from './types.js';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

type UmbMediaConfigurationResponse = UmbRepositoryResponse<UmbMediaConfigurationModel>;

let configurationPromise: Promise<UmbMediaConfigurationResponse> | undefined;

/**
 * Returns the cached media configuration, or requests it if it has not been fetched yet.
 * Concurrent calls share one request and error responses are not cached.
 * @param {() => Promise<UmbMediaConfigurationResponse>} request - Fetches the configuration when nothing is cached.
 * @returns {Promise<UmbMediaConfigurationResponse>} - The media configuration.
 * @internal
 */
export async function _getCachedMediaConfiguration(
	request: () => Promise<UmbMediaConfigurationResponse>,
): Promise<UmbMediaConfigurationResponse> {
	configurationPromise ??= request();
	const response = await configurationPromise;
	if (response.error) {
		configurationPromise = undefined;
	}
	return response;
}

/**
 * Clears the cached media configuration.
 * @internal
 */
export function _resetMediaConfigurationCacheForTesting(): void {
	configurationPromise = undefined;
}
