import type { ManifestBase } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { ReplaySubject } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Base class for extension initializers, which are responsible for loading and unloading extensions.
 */
export abstract class UmbExtensionInitializerBase<
	Key extends string,
	T extends ManifestBase = SpecificManifestTypeOrManifestBase<UmbExtensionManifest, Key>,
> extends UmbControllerBase {
	protected host;
	protected extensionRegistry;
	#extensionMap = new Map();
	#loaded = new ReplaySubject<void>(1);
	loaded = this.#loaded.asObservable();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<T>, manifestType: Key) {
		super(host);
		this.host = host;
		this.extensionRegistry = extensionRegistry;
		this.observe(extensionRegistry.byType<Key, T>(manifestType), async (extensions) => {
			this.#extensionMap.forEach((existingExt) => {
				if (!extensions.find((b) => b.alias === existingExt.alias)) {
					this.unloadExtension(existingExt);
					this.#extensionMap.delete(existingExt.alias);
				}
			});

			await Promise.all(
				extensions.map((extension) => {
					if (this.#extensionMap.has(extension.alias)) return;
					this.#extensionMap.set(extension.alias, extension);
					return this.instantiateExtension(extension);
				}),
			);

			this.#loaded.next();
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
