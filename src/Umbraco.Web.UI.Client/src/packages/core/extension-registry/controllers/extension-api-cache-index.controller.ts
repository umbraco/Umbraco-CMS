import { UmbExtensionApiCacheIndexBase, type UmbExtensionApiOf } from './extension-api-cache-index-base.controller.js';
import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Looks up the first extension of a given type that matches a key, and creates its API.
 *
 * What a key matches is declared by the filter given on construction, for example an extension that
 * is registered for a given alias. The lookup is cached per key, including when there is none, so the
 * extension registry is only searched once per key. The cache is never invalidated, which suits
 * short-lived owners that process a batch of values and are then discarded. Each call to
 * {@link UmbExtensionApiCacheIndex.createApi} still creates a new API instance, so instances are never
 * shared between callers. To use every matching extension, see `UmbExtensionApisCacheIndex`.
 * @template {ManifestApi} ManifestType The manifest type of the extensions to look up.
 * @template KeyType The type of the key the extensions are looked up by.
 */
export class UmbExtensionApiCacheIndex<
	ManifestType extends ManifestApi<UmbApi>,
	KeyType = string,
> extends UmbExtensionApiCacheIndexBase<ManifestType, KeyType> {
	/**
	 * Gets the manifest that matches a key.
	 * @param {KeyType} key - The key to look up.
	 * @returns {ManifestType | undefined} The first matching manifest, or undefined when there is none.
	 */
	getManifest(key: KeyType): ManifestType | undefined {
		return this._getManifests(key)[0];
	}

	/**
	 * Creates a new API instance for the extension that matches a key.
	 * The manifest is set on the API as `manifest`.
	 * @param {KeyType} key - The key to look up.
	 * @returns {Promise<UmbExtensionApiOf<ManifestType> | undefined>} The API, or undefined when there is no matching extension or its API could not be created.
	 */
	async createApi(key: KeyType): Promise<UmbExtensionApiOf<ManifestType> | undefined> {
		const manifest = this.getManifest(key);
		return manifest ? await this._createApi(manifest) : undefined;
	}
}
