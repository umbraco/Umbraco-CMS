import { PackageResource, OpenAPI } from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ManifestBase, isManifestJSType } from '@umbraco-cms/backoffice/extension-api';

// TODO: consider if this can be replaced by the new extension controllers
// TODO: move local part out of this, and name something with server.
export class UmbServerExtensionRegistrator extends UmbBaseController {
	#extensionRegistry: UmbBackofficeExtensionRegistry;
	#apiBaseUrl = OpenAPI.BASE;

	constructor(host: UmbControllerHost, extensionRegistry: UmbBackofficeExtensionRegistry) {
		super(host, UmbServerExtensionRegistrator.name);
		this.#extensionRegistry = extensionRegistry;
		// TODO: This was before in hostConnected(), but I don't see the reason to wait. lets just do it right away.
		this.#loadServerPackages();
	}

	async #loadServerPackages() {
		/* TODO: we need a new endpoint here, to remove the dependency on the package repository, to get the modules available for the backoffice scope
		/ we will need a similar endpoint for the login, installer etc at some point.
			We should expose more information about the packages when not authorized so the end point should only return a list of modules from the manifest with
			with the correct scope.

			This code is copy pasted from the package repository. We probably don't need this is the package repository anymore.
		*/
		const { data: packages } = await tryExecuteAndNotify(this._host, PackageResource.getPackageManifest());

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
