import type { ManifestTypes } from './models/index.js';
import { umbExtensionsRegistry } from './registry.js';
import {
	createExtensionApi,
	ManifestBase,
	ManifestApi,
	SpecificManifestTypeOrManifestBase,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * Initializes a extensions APIs for a host element.
 * The Extension API will be given one argument, the host element.
 *
 * @param host The host element to initialize extension classes for.
 * @param extensionType The extension type(strings) to initialize.
 * @param extensionAlias The extension alias to target.
 *
 */
export class UmbExtensionApiInitializer<
	ExtensionType extends string = string,
	ExtensionManifest extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ExtensionType>,
	ExtensionApiInterface = ExtensionManifest extends ManifestApi ? ExtensionManifest['API_TYPE'] : unknown
> extends UmbBaseController {
	#currentPromise?: Promise<ExtensionApiInterface | undefined>;
	#currentPromiseResolver?: (value: ExtensionApiInterface | undefined) => void;
	#currentApi?: ExtensionApiInterface;

	constructor(
		host: UmbControllerHostElement,
		extensionType: ExtensionType,
		extensionAlias: string,
		callback: (extensionApi: ExtensionApiInterface | undefined) => void
	) {
		super(host);
		const source = umbExtensionsRegistry.getByTypeAndAlias(extensionType, extensionAlias);
		//TODO: The promise can probably be done in a cleaner way.
		this.observe(source, async (manifest) => {
			if (!manifest) return;

			try {
				// Destroy the previous class if it exists, and if destroy method is an method on the class.
				(this.#currentApi as any)?.destroy?.();

				this.#currentApi = await createExtensionApi<ExtensionApiInterface>(manifest, [host]);
				callback(this.#currentApi);
				if (this.#currentPromiseResolver) {
					this.#currentPromiseResolver(this.#currentApi);
					this.#currentPromise = undefined;
					this.#currentPromiseResolver = undefined;
				}
			} catch (error) {
				throw new Error(`Could not create class of extension type '${extensionType}' with alias '${extensionAlias}'`);
			}
		});
	}

	public asPromise() {
		this.#currentPromise ??= new Promise((resolve) => {
			this.#currentPromiseResolver = resolve;
		});
		return this.#currentPromise;
	}

	public destroy(): void {
		super.destroy();
		// Destroy the current class if it exists, and if destroy method is an method on the class.
		(this.#currentApi as any)?.destroy?.();
	}
}
