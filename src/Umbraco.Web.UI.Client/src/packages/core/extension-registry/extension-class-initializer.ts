import type { ManifestTypes } from './models';
import { umbExtensionsRegistry } from './registry';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import {
	createExtensionClass,
	ManifestBase,
	ManifestClass,
	SpecificManifestTypeOrManifestBase,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbExtensionClassInitializer<
	ExtensionType extends string = string,
	ExtensionManifest extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ExtensionType>,
	ExtensionClassInterface = ExtensionManifest extends ManifestClass ? ExtensionManifest['CLASS_TYPE'] : unknown
> {
	#observable;
	#currentPromise?: Promise<ExtensionClassInterface | undefined>;
	#currentPromiseResolver?: (value: ExtensionClassInterface | undefined) => void;

	constructor(
		host: UmbControllerHostElement,
		extensionType: ExtensionType,
		extensionAlias: string,
		callback: (extensionClass: ExtensionClassInterface | undefined) => void
	) {
		const source = umbExtensionsRegistry.getByTypeAndAlias(extensionType, extensionAlias);
		//TODO: The promise can probably be done in a cleaner way.
		this.#observable = new UmbObserverController(host, source, async (manifest) => {
			if (!manifest) return;

			try {
				const initializedClass = await createExtensionClass<ExtensionClassInterface>(manifest, [host]);
				callback(initializedClass);
				if (this.#currentPromiseResolver) {
					this.#currentPromiseResolver(initializedClass);
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
}
