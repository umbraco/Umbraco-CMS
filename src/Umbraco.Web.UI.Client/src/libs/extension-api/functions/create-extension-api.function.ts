import type { ApiLoaderProperty } from '../types/utils.js';
import type { ManifestApi, ManifestElementAndApi } from '../types/base.types.js';
import type { UmbApi } from '../models/api.interface.js';
import type { UmbApiConstructorArgumentsMethodType } from './types.js';
import { loadManifestApi } from './load-manifest-api.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
 * @param {ManifestApi} manifest - The manifest of the extension
 * @param {Array | UmbApiConstructorArgumentsMethodType} constructorArgs - The constructor arguments to pass to the API class
 * @param {ApiLoaderProperty} fallbackApi - A fallback API loader property to use if the manifest does not have one
 * @returns {Promise<UmbApi | undefined>} - The API class instance
 */
export async function createExtensionApi<ApiType extends UmbApi = UmbApi>(
	host: UmbControllerHost,
	manifest: ManifestApi<ApiType> | ManifestElementAndApi<any, ApiType>,
	constructorArgs?:
		| Array<unknown>
		| UmbApiConstructorArgumentsMethodType<ManifestApi<ApiType> | ManifestElementAndApi<any, ApiType>>,
	fallbackApi?: ApiLoaderProperty<ApiType>,
): Promise<ApiType | undefined> {
	const apiPropValue = manifest.api ?? manifest.js ?? fallbackApi;
	if (!apiPropValue) {
		console.error(
			`-- Extension of alias "${manifest.alias}" did not succeed creating an API class instance, missing a JavaScript file via the 'api' or 'js' property, using either an 'api' or 'default' export.`,
			manifest,
		);
		return undefined;
	}

	const apiConstructor = await loadManifestApi<ApiType>(apiPropValue);
	if (apiConstructor) {
		const additionalArgs = (typeof constructorArgs === 'function' ? constructorArgs(manifest) : constructorArgs) ?? [];
		return new apiConstructor(host, ...additionalArgs);
	}

	console.error(
		`-- Extension of alias "${manifest.alias}" did not succeed instantiating an API class instance via the extension manifest 'api' or 'js' property, using either an 'api' or 'default' export.`,
		manifest,
	);

	return undefined;
}
