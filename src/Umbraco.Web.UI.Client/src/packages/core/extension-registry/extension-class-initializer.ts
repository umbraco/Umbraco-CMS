import type { ManifestTypes } from './models/index.js';
import { umbExtensionsRegistry } from './registry.js';
import {
	createExtensionClass,
	ManifestBase,
	ManifestClass,
	SpecificManifestTypeOrManifestBase,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbExtensionClassInitializer<
	ExtensionType extends string = string,
	ExtensionManifest extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ExtensionType>,
	ExtensionClassInterface = ExtensionManifest extends ManifestClass ? ExtensionManifest['CLASS_TYPE'] : unknown
> extends UmbBaseController {
	#currentPromise?: Promise<ExtensionClassInterface | undefined>;
	#currentPromiseResolver?: (value: ExtensionClassInterface | undefined) => void;
	#currentClass?: ExtensionClassInterface;

	constructor(
		host: UmbControllerHostElement,
		extensionType: ExtensionType,
		extensionAlias: string,
		callback: (extensionClass: ExtensionClassInterface | undefined) => void
	) {
		super(host);
		const source = umbExtensionsRegistry.getByTypeAndAlias(extensionType, extensionAlias);
		//TODO: The promise can probably be done in a cleaner way.
		this.observe(source, async (manifest) => {
			if (!manifest) return;

			try {
				// Destroy the previous class if it exists, and if destroy method is an method on the class.
				(this.#currentClass as any)?.destroy?.();

				this.#currentClass = await createExtensionClass<ExtensionClassInterface>(manifest, [host]);
				callback(this.#currentClass);
				if (this.#currentPromiseResolver) {
					this.#currentPromiseResolver(this.#currentClass);
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
		(this.#currentClass as any)?.destroy?.();
	}
}
