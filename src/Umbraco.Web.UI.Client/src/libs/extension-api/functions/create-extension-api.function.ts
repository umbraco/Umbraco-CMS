import type { UmbApi } from '../models/api.interface.js';
import type { ManifestApi, ManifestElementAndApi } from '../types/base.types.js';
import { loadManifestApi } from './load-manifest-api.function.js';
import type { UmbApiConstructorArgumentsMethodType } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
 * @param {ManifestApi} manifest - The manifest of the extension
 * @param {Array | UmbApiConstructorArgumentsMethodType} constructorArgs - The constructor arguments to pass to the API class
 * @returns {Promise<UmbApi | undefined>} - The API class instance
 */
export async function createExtensionApi<ApiType extends UmbApi = UmbApi>(
	host: UmbControllerHost,
	manifest: ManifestApi<ApiType> | ManifestElementAndApi<any, ApiType>,
	constructorArgs?:
		| Array<unknown>
		| UmbApiConstructorArgumentsMethodType<ManifestApi<ApiType> | ManifestElementAndApi<any, ApiType>>,
): Promise<ApiType | undefined> {
	if (manifest.api) {
		const apiConstructor = await loadManifestApi<ApiType>(manifest.api);
		if (apiConstructor) {
			const additionalArgs =
				(typeof constructorArgs === 'function' ? constructorArgs(manifest) : constructorArgs) ?? [];
			return new apiConstructor(host, ...additionalArgs);
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed instantiate a API class via the extension manifest property 'api', using either a 'api' or 'default' export`,
				manifest,
			);
		}
	}

	if (manifest.js) {
		const apiConstructor2 = await loadManifestApi<ApiType>(manifest.js);
		if (apiConstructor2) {
			const additionalArgs =
				(typeof constructorArgs === 'function' ? constructorArgs(manifest) : constructorArgs) ?? [];
			return new apiConstructor2(host, ...additionalArgs);
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed instantiate a API class via the extension manifest property 'js', using either a 'api' or 'default' export`,
				manifest,
			);
		}
	}

	console.error(
		`-- Extension of alias "${manifest.alias}" did not succeed creating an api class instance, missing a JavaScript file via the 'api' or 'js' property.`,
		manifest,
	);

	return undefined;
}
