import { umbExtensionsRegistry } from '../registry.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { createExtensionApi, type ManifestApi, type UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from 'src/libs/class-api/controller-base.class.js';

type ApiOf<ManifestType> = ManifestType extends ManifestApi<infer ApiType> ? ApiType : never;

/**
 * Looks up the extension of a given type that matches a key, and creates its API.
 *
 * What a key matches is declared by the filter given on construction, for example an extension that
 * is registered for a given alias. The manifest lookup is cached per key, including when there is none,
 * so the extension registry is only searched once per key. The cache is never invalidated, which suits
 * short-lived owners that process a batch of values and are then discarded. Each call to
 * {@link UmbExtensionApiCacheIndex.createApi} still creates a new API instance, so instances are never
 * shared between callers.
 * @template {ManifestApi} ManifestType The manifest type of the extensions to look up.
 * @template KeyType The type of the key the extensions are looked up by.
 */
export class UmbExtensionApiCacheIndex<
	ManifestType extends ManifestApi<UmbApi>,
	KeyType = string,
> extends UmbControllerBase {
	readonly #extensionType: ManifestType['type'];
	readonly #filter: (manifest: ManifestType, key: KeyType) => boolean;
	readonly #manifests = new Map<KeyType, ManifestType | undefined>();

	/**
	 * @param {UmbControllerHost} host - The host the created APIs are given.
	 * @param {string} extensionType - The type of the extensions to look up.
	 * @param {(manifest: ManifestType, key: KeyType) => boolean} filter - Tells whether a manifest of that type matches a key.
	 */
	constructor(
		host: UmbControllerHost,
		extensionType: ManifestType['type'],
		filter: (manifest: ManifestType, key: KeyType) => boolean,
	) {
		super(host);
		this.#extensionType = extensionType;
		this.#filter = filter;
	}

	/**
	 * Gets the manifest that matches a key.
	 * @param {KeyType} key - The key to look up.
	 * @returns {ManifestType | undefined} The first matching manifest, or undefined when there is none.
	 */
	getManifest(key: KeyType): ManifestType | undefined {
		if (!this.#manifests.has(key)) {
			this.#manifests.set(
				key,
				umbExtensionsRegistry.getByTypeAndFilter<string, ManifestType>(this.#extensionType, (manifest) =>
					this.#filter(manifest, key),
				)[0],
			);
		}
		return this.#manifests.get(key);
	}

	/**
	 * Creates a new API instance for the extension that matches a key.
	 * The manifest is set on the API as `manifest`.
	 * @param {KeyType} key - The key to look up.
	 * @returns {Promise<ApiOf<ManifestType> | undefined>} The API, or undefined when there is no matching extension or its API could not be created.
	 */
	async createApi(key: KeyType): Promise<ApiOf<ManifestType> | undefined> {
		const manifest = this.getManifest(key);
		if (!manifest) {
			return undefined;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return undefined;
		}

		(api as { manifest?: ManifestType }).manifest = manifest;
		return api as ApiOf<ManifestType>;
	}
}
