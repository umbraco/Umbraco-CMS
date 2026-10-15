import type { UmbDataTypeConfigurationModel } from './types.js';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

type UmbDataTypeConfigurationResponse = UmbRepositoryResponse<UmbDataTypeConfigurationModel>;

let configurationPromise: Promise<UmbDataTypeConfigurationResponse> | undefined;

/**
 * Returns the cached data type configuration, or requests it if it has not been fetched yet.
 * Concurrent calls share one request and error responses are not cached.
 * @param {() => Promise<UmbDataTypeConfigurationResponse>} request - Fetches the configuration when nothing is cached.
 * @returns {Promise<UmbDataTypeConfigurationResponse>} - The data type configuration.
 * @internal
 */
export async function _getCachedDataTypeConfiguration(
	request: () => Promise<UmbDataTypeConfigurationResponse>,
): Promise<UmbDataTypeConfigurationResponse> {
	configurationPromise ??= request();
	const response = await configurationPromise;
	if (response.error) {
		configurationPromise = undefined;
	}
	return response;
}

/**
 * Clears the cached data type configuration.
 * @internal
 */
export function _resetDataTypeConfigurationCacheForTesting(): void {
	configurationPromise = undefined;
}
