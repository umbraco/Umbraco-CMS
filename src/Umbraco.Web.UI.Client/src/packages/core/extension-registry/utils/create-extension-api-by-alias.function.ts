import { umbExtensionsRegistry } from '../registry.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { createExtensionApi, type UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 *
 * @param {UmbControllerHost} host  - The host to parse on as the host to the extension api.
 * @param {string} alias - The alias of the extension api to create.
 * @param {Array<unknown>} constructorArgs - The constructor arguments to pass to the extension api, host will always be appended as the first argument, meaning these arguments will be second and so forth.
 * @returns {ApiType} a class instance of the api provided via the manifest that matches the given alias. You have to type this via the generic `ApiType` parameter.
 */
export async function createExtensionApiByAlias<ApiType extends UmbApi = UmbApi>(
	host: UmbControllerHost,
	alias: string,
	constructorArgs?: Array<unknown>,
): Promise<ApiType> {
	// Get manifest:
	const manifest = umbExtensionsRegistry.getByAlias(alias);
	if (!manifest) {
		throw new Error(`Failed to get manifest by alias: ${alias}`);
	}

	const api = await createExtensionApi<ApiType>(host, manifest, constructorArgs);
	if (!api) {
		throw new Error(`Failed to create extension api from alias: ${alias}`);
	}
	// Create extension:
	return api;
}
