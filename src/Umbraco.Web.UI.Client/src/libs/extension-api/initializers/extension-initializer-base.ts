import type { ManifestBase } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

/**
 * Base class for extension initializers, which are responsible for loading and unloading extensions.
 */
export abstract class UmbExtensionInitializerBase<
	Key extends string,
	T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>,
> extends UmbControllerBase {
	protected host;
	protected extensionRegistry;
	#extensionMap = new Map();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<T>, manifestType: Key) {
		super(host);
		this.host = host;
		this.extensionRegistry = extensionRegistry;
		this.observe(extensionRegistry.byType<Key, T>(manifestType), (extensions) => {
			this.#extensionMap.forEach((existingExt) => {
				if (!extensions.find((b) => b.alias === existingExt.alias)) {
					this.unloadExtension(existingExt);
					this.#extensionMap.delete(existingExt.alias);
				}
			});

			extensions.forEach((extension) => {
				if (this.#extensionMap.has(extension.alias)) return;
				this.#extensionMap.set(extension.alias, extension);
				this.instantiateExtension(extension);
			});
		});
	}

	/**
	 * Perform any logic required to instantiate the extension.
	 */
	abstract instantiateExtension(manifest: T): Promise<void> | void;

	/**
	 * Perform any logic required to unload the extension.
	 */
	abstract unloadExtension(manifest: T): Promise<void> | void;
}
