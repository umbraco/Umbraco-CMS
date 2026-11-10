import { umbExtensionsRegistry } from '../registry.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { createExtensionApi, type ManifestApi, type UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type UmbExtensionApiOf<ManifestType> = ManifestType extends ManifestApi<infer ApiType> ? ApiType : never;

/**
 * Base for looking up the extensions of a given type that match a key, and creating their APIs.
 *
 * What a key matches is declared by the filter given on construction, for example an extension that
 * is registered for a given alias. The lookup is cached per key, including when nothing matches, so the
 * extension registry is only searched once per key. The cache is never invalidated, which suits short-lived
 * owners that process a batch of values and are then discarded. The created APIs are given this controller as
 * their host, and a new API is created on every request, so API instances are never shared between callers.
 * @template {ManifestApi} ManifestType The manifest type of the extensions to look up.
 * @template KeyType The type of the key the extensions are looked up by.
 */
export abstract class UmbExtensionApiCacheIndexBase<
	ManifestType extends ManifestApi<UmbApi>,
	KeyType = string,
> extends UmbControllerBase {
	readonly #extensionType: ManifestType['type'];
	readonly #filter: (manifest: ManifestType, key: KeyType) => boolean;
	readonly #getCacheKey?: (key: KeyType) => string | number;
	readonly #manifests = new Map<unknown, Array<ManifestType>>();

	/**
	 * @param {UmbControllerHost} host - The host of this controller, which in turn hosts the created APIs.
	 * @param {string} extensionType - The type of the extensions to look up.
	 * @param {(manifest: ManifestType, key: KeyType) => boolean} filter - Tells whether a manifest of that type matches a key.
	 * @param {(key: KeyType) => string | number} [getCacheKey] - Turns a key into the value the lookup is cached by. Needed when
	 * keys are objects, which would otherwise be cached by reference. By default, the key itself is used.
	 */
	constructor(
		host: UmbControllerHost,
		extensionType: ManifestType['type'],
		filter: (manifest: ManifestType, key: KeyType) => boolean,
		getCacheKey?: (key: KeyType) => string | number,
	) {
		super(host);
		this.#extensionType = extensionType;
		this.#filter = filter;
		this.#getCacheKey = getCacheKey;
	}

	/**
	 * Gets the manifests that match a key, cached per key.
	 * @param {KeyType} key - The key to look up.
	 * @returns {Array<ManifestType>} The matching manifests in registry order, empty when there are none.
	 */
	protected _getManifests(key: KeyType): Array<ManifestType> {
		const cacheKey = this.#getCacheKey ? this.#getCacheKey(key) : key;
		let manifests = this.#manifests.get(cacheKey);
		if (!manifests) {
			manifests = umbExtensionsRegistry.getByTypeAndFilter<string, ManifestType>(this.#extensionType, (manifest) =>
				this.#filter(manifest, key),
			);
			this.#manifests.set(cacheKey, manifests);
		}
		return manifests;
	}

	/**
	 * Creates a new API instance for a manifest, and sets the manifest on it as `manifest`.
	 * @param {ManifestType} manifest - The manifest to create the API for.
	 * @returns {Promise<UmbExtensionApiOf<ManifestType> | undefined>} The API, or undefined when it could not be created.
	 */
	protected async _createApi(manifest: ManifestType): Promise<UmbExtensionApiOf<ManifestType> | undefined> {
		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return undefined;
		}

		(api as { manifest?: ManifestType }).manifest = manifest;
		return api as UmbExtensionApiOf<ManifestType>;
	}
}
