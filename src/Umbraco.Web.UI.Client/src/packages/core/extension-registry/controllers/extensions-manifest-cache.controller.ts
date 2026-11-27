import { umbExtensionsRegistry } from '../registry.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

/**
 * Looks up the extensions of a given type that match a key.
 *
 * What a key matches is declared by the filter given on construction, for example an extension that
 * is registered for a given alias. The lookup is cached per key, including when nothing matches, so the
 * extension registry is only searched once per key. The cache is never invalidated, which suits
 * short-lived owners that process a batch of values and are then discarded.
 * Consumers that only need one extension can use the first manifest.
 * @template {ManifestBase} ManifestType The manifest type of the extensions to look up.
 * @template KeyType The type of the key the extensions are looked up by.
 */
export class UmbExtensionsManifestCache<ManifestType extends ManifestBase, KeyType = string> extends UmbControllerBase {
	readonly #extensionType: ManifestType['type'];
	readonly #filter: (manifest: ManifestType, key: KeyType) => boolean;
	readonly #getCacheKey?: (key: KeyType) => string | number;
	readonly #manifests = new Map<unknown, Array<ManifestType>>();

	/**
	 * @param {UmbControllerHost} host - The host of this controller.
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
	 * @returns {ReadonlyArray<ManifestType>} The matching manifests in registry order, empty when there are none.
	 */
	getManifests(key: KeyType): ReadonlyArray<ManifestType> {
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
}
