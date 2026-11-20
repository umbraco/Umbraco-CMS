import type { ApiLoaderProperty } from '../types/utils.js';
import type { ManifestApi, ManifestElementAndApi } from '../types/base.types.js';
import type { UmbApi } from '../models/api.interface.js';
import type { UmbApiConstructorArgumentsMethodType } from './types.js';
import { createExtensionApi } from './create-extension-api.function.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Creates an API instance for each of the given manifests, in parallel, using {@link createExtensionApi}.
 *
 * Like {@link createExtensionApi}, an API that could not be created is logged and resolves to `undefined`.
 * The result has one entry per manifest, in the same order, so an entry can be paired with its manifest by index.
 * An error thrown while creating an API rejects the returned promise.
 * @param {UmbControllerHost} host - The controller host the APIs are created for.
 * @param {Array<ManifestApi>} manifests - The manifests of the extensions to create the APIs of.
 * @param {Array | UmbApiConstructorArgumentsMethodType} constructorArgs - The constructor arguments to pass to every API class.
 * @param {ApiLoaderProperty} fallbackApi - A fallback API loader property to use for manifests that do not have one.
 * @returns {Promise<Array<UmbApi | undefined>>} - The API instances, in the order of the manifests.
 */
export async function createExtensionApis<ApiType extends UmbApi = UmbApi>(
	host: UmbControllerHost,
	manifests: ReadonlyArray<ManifestApi<ApiType> | ManifestElementAndApi<any, ApiType>>,
	constructorArgs?:
		| Array<unknown>
		| UmbApiConstructorArgumentsMethodType<ManifestApi<ApiType> | ManifestElementAndApi<any, ApiType>>,
	fallbackApi?: ApiLoaderProperty<ApiType>,
): Promise<Array<ApiType | undefined>> {
	return await Promise.all(
		manifests.map((manifest) => createExtensionApi<ApiType>(host, manifest, constructorArgs, fallbackApi)),
	);
}
