import { map } from '@umbraco-cms/backoffice/external/rxjs';
import {
	ManifestBase,
	ManifestTypeMap,
	SpecificManifestTypeOrManifestBase,
	UmbBaseExtensionController,
	UmbExtensionRegistry,
} from '@umbraco-cms/backoffice/extension-api';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export abstract class UmbBaseExtensionsController<
	ManifestTypeName extends keyof ManifestTypeMap<ManifestTypes> | string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbBaseExtensionController<ManifestType> = UmbBaseExtensionController<ManifestType>,
	PermittedControllerType extends ControllerType = ControllerType
> extends UmbBaseController {
	#onChange: (permittedManifests: Array<PermittedControllerType>, controller: PermittedControllerType) => void;
	private _extensions: Array<ControllerType> = [];
	private _permittedExts: Array<PermittedControllerType> = [];

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestType>,
		type: ManifestTypeName,
		filter: undefined | null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<PermittedControllerType>, controller: PermittedControllerType) => void
	) {
		super(host);
		this.#onChange = onChange;

		// TODO: This could be optimized by just getting the aliases, well depends on the filter. (revisit one day to see how much filter is used)
		let source = extensionRegistry.extensionsOfType<ManifestTypeName, ManifestType>(type);
		if (filter) {
			source = source.pipe(map((extensions: Array<ManifestType>) => extensions.filter(filter)));
		}
		this.observe(source, this.#gotManifests);
	}

	#gotManifests = (manifests: Array<ManifestType>) => {
		if (!manifests) {
			// Clean up:
			this._extensions.forEach((controller) => {
				controller.destroy();
			});
			this._extensions = [];
			// _permittedExts should have been cleared via the destroy callbacks.
			return;
		}

		// Clean up extensions that are no longer.
		this._extensions = this._extensions.filter((controller) => {
			if (!manifests.find((manifest) => manifest.alias === controller.alias)) {
				controller.destroy();
				// destroying the controller will, if permitted, make a last callback with isPermitted = false. This will also remove it from the _permittedExts array.
				return false;
			}
			return true;
		});

		// ---------------------------------------------------------------
		// May change this into a Extensions Manager Controller???
		// ---------------------------------------------------------------

		manifests.forEach((manifest) => {
			const existing = this._extensions.find((x) => x.alias === manifest.alias);
			if (!existing) {
				// Idea: could be abstracted into a createController method, so we can override it in a subclass.
				// (This should be enough to be able to create a element extension controller instead.)

				this._extensions.push(this._createController(manifest));
			}
		});
	};

	protected abstract _createController(manifest: ManifestType): ControllerType;

	protected _extensionChanged = (isPermitted: boolean, controller: ControllerType) => {
		let hasChanged = false;
		const existingIndex = this._permittedExts.indexOf(controller as PermittedControllerType);
		if (isPermitted) {
			if (existingIndex === -1) {
				this._permittedExts.push(controller as PermittedControllerType);
				hasChanged = true;
			}
		} else {
			if (existingIndex !== -1) {
				this._permittedExts.splice(existingIndex, 1);
				hasChanged = true;
			}
		}
		if (hasChanged) {
			// The final list of permitted extensions to be displayed, this will be stripped from extensions that are overwritten by another extension and sorted accordingly.
			const exposedPermittedExts = [...this._permittedExts];

			// Removal of overwritten extensions:
			this._permittedExts.forEach((extCtrl) => {
				// Check if it overwrites another extension:
				// if so, look up the extension it overwrites, and remove it from the list. and check that for if it overwrites another extension and so on.
				if (extCtrl.overwrites.length > 0) {
					extCtrl.overwrites.forEach((overwrite) => {
						this.#removeOverwrittenExtensions(exposedPermittedExts, overwrite);
					});
				}
			});

			// Sorting:
			exposedPermittedExts.sort((a, b) => b.weight - a.weight);

			this.#onChange(exposedPermittedExts, this as unknown as PermittedControllerType);
		}
	};

	#removeOverwrittenExtensions(list: Array<PermittedControllerType>, alias: string) {
		const index = list.findIndex((a) => a.alias === alias);
		if (index !== -1) {
			const entry = list[index];
			// Remove this extension:
			list.splice(index, 1);
			// Then remove other extensions that this was replacing:
			if (entry.overwrites.length > 0) {
				entry.overwrites.forEach((overwrite) => {
					this.#removeOverwrittenExtensions(list, overwrite);
				});
			}
		}
	}

	public destroy() {
		super.destroy();
		this._extensions = [];
		this._permittedExts = [];
	}
}
