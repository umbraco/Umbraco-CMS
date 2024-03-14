import type { UmbApi } from '../models/api.interface.js';
import { createExtensionApi } from './create-extension-api.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export async function createExtensionApiByAlias<ApiType extends UmbApi = UmbApi>(
	host: UmbControllerHost,
	constructorArgs?: Array<unknown>,
): Promise<ApiType | undefined> {
	// Get Manifest:
	const manifest = {};

	// Create extension:
	return createExtensionApi(host, manifest, constructorArgs);
}
