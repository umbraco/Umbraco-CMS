import type { UmbDocumentConfigurationModel } from './types.js';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

type UmbDocumentConfigurationResponse = UmbRepositoryResponse<UmbDocumentConfigurationModel>;

let configurationPromise: Promise<UmbDocumentConfigurationResponse> | undefined;

/**
 * Returns the cached document configuration, or requests it if it has not been fetched yet.
 * Concurrent calls share one request and error responses are not cached.
 * @param {() => Promise<UmbDocumentConfigurationResponse>} request - Fetches the configuration when nothing is cached.
 * @returns {Promise<UmbDocumentConfigurationResponse>} - The document configuration.
 * @internal
 */
export async function _getCachedDocumentConfiguration(
	request: () => Promise<UmbDocumentConfigurationResponse>,
): Promise<UmbDocumentConfigurationResponse> {
	configurationPromise ??= request();
	const response = await configurationPromise;
	if (response.error) {
		configurationPromise = undefined;
	}
	return response;
}

/**
 * Clears the cached document configuration.
 * @internal
 */
export function _resetDocumentConfigurationCacheForTesting(): void {
	configurationPromise = undefined;
}
