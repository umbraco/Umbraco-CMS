import { Subject } from 'rxjs';
import { UmbController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { PackageResource, OpenAPI } from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ManifestBase, isManifestJSType } from '@umbraco-cms/backoffice/extension-api';

export class UmbExtensionInitializer extends UmbController {
	host: UmbControllerHostElement;
	#extensionRegistry: UmbBackofficeExtensionRegistry;
	#unobserve = new Subject<void>();
	#localPackages: Array<Promise<any>> = [];
	#apiBaseUrl = OpenAPI.BASE;

	constructor(host: UmbControllerHostElement, extensionRegistry: UmbBackofficeExtensionRegistry) {
		super(host, UmbExtensionInitializer.name);
		this.host = host;
		this.#extensionRegistry = extensionRegistry;
	}

	setLocalPackages(localPackages: Array<Promise<any>>) {
		this.#localPackages = localPackages;
		this.#loadLocalPackages();
	}

	hostConnected(): void {
		this.#loadServerPackages();
	}

	hostDisconnected(): void {
		this.#unobserve.next();
		this.#unobserve.complete();
	}

	async #loadLocalPackages() {
		this.#localPackages.forEach(async (packageImport) => {
			const packageModule = await packageImport;
			this.#extensionRegistry.registerMany(packageModule.extensions);
		});
	}

	async #loadServerPackages() {
		/* TODO: we need a new endpoint here, to remove the dependency on the package repository, to get the modules available for the backoffice scope
		/ we will need a similar endpoint for the login, installer etc at some point. 
			We should expose more information about the packages when not authorized so the end point should only return a list of modules from the manifest with
			with the correct scope.

			This code is copy pasted from the package repository. We probably don't need this is the package repository anymore.
		*/
		const { data: packages } = await tryExecuteAndNotify(this.host, PackageResource.getPackageManifest());

		if (packages) {
			// Append packages to the store but only if they have a name
			//store.appendItems(packages.filter((p) => p.name?.length));
			const extensions: ManifestBase[] = [];

			packages.forEach((p) => {
				p.extensions?.forEach((e) => {
					// Crudely validate that the extension at least follows a basic manifest structure
					// Idea: Use `Zod` to validate the manifest
					if (this.isManifestBase(e)) {
						/**
						 * Crude check to see if extension is of type "js" since it is safe to assume we do not
						 * need to load any other types of extensions in the backoffice (we need a js file to load)
						 */
						if (isManifestJSType(e)) {
							// Add API base url if the js path is relative
							if (!e.js.startsWith('http')) {
								e.js = `${this.#apiBaseUrl}${e.js}`;
							}
						}

						extensions.push(e);
					}
				});
			});

			this.#extensionRegistry.registerMany(extensions);
		}
	}

	private isManifestBase(x: unknown): x is ManifestBase {
		return typeof x === 'object' && x !== null && 'alias' in x;
	}
}
