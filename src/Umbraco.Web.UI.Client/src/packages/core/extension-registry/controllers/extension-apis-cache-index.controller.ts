import { UmbExtensionApiCacheIndexBase, type UmbExtensionApiOf } from './extension-api-cache-index-base.controller.js';
import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Looks up every extension of a given type that matches a key, and creates their APIs.
 *
 * What a key matches is declared by the filter given on construction, for example an extension that
 * is registered for a given alias. The lookup is cached per key, including when nothing matches, so the
 * extension registry is only searched once per key. The cache is never invalidated, which suits
 * short-lived owners that process a batch of values and are then discarded. Each call to
 * {@link UmbExtensionApisCacheIndex.createApis} still creates new API instances, so instances are never
 * shared between callers. To use only the first matching extension, see `UmbExtensionApiCacheIndex`.
 * @template {ManifestApi} ManifestType The manifest type of the extensions to look up.
 * @template KeyType The type of the key the extensions are looked up by.
 */
export class UmbExtensionApisCacheIndex<
	ManifestType extends ManifestApi<UmbApi>,
	KeyType = string,
> extends UmbExtensionApiCacheIndexBase<ManifestType, KeyType> {
	/**
	 * Gets the manifests that match a key.
	 * @param {KeyType} key - The key to look up.
	 * @returns {ReadonlyArray<ManifestType>} The matching manifests in registry order, empty when there are none.
	 */
	getManifests(key: KeyType): ReadonlyArray<ManifestType> {
		return this._getManifests(key);
	}

	/**
	 * Creates new API instances for the extensions that match a key, in parallel.
	 * The manifest is set on each API as `manifest`.
	 * @param {KeyType} key - The key to look up.
	 * @returns {Promise<Array<UmbExtensionApiOf<ManifestType>>>} The APIs in registry order, without those that could not be created.
	 */
	async createApis(key: KeyType): Promise<Array<UmbExtensionApiOf<ManifestType>>> {
		const apis = await Promise.all(this._getManifests(key).map((manifest) => this._createApi(manifest)));
		return apis.filter((api) => api !== undefined) as Array<UmbExtensionApiOf<ManifestType>>;
	}
}
