import { map } from '@umbraco-cms/backoffice/external/rxjs';
import {
	ManifestBase,
	ManifestTypeMap,
	SpecificManifestTypeOrManifestBase,
	UmbBaseExtensionController,
} from '@umbraco-cms/backoffice/extension-api';
import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export abstract class UmbBaseExtensionsController<
	ManifestTypeName extends keyof ManifestTypeMap<ManifestTypes> | string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbBaseExtensionController<ManifestType> = UmbBaseExtensionController<ManifestType>
> extends UmbBaseController {
	#onChange: (permittedManifests: Array<ControllerType>, controller: ControllerType) => void;
	private _extensions: Array<ControllerType> = [];
	private _permittedExts: Array<ControllerType> = [];

	get permittedExtensions(): Array<ControllerType> {
		return this._permittedExts;
	}

	constructor(
		host: UmbControllerHost,
		type: ManifestTypeName,
		filter: null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<ControllerType>, controller: ControllerType) => void
	) {
		super(host);
		this.#onChange = onChange;

		// TODO: This could be optimized by just getting the aliases, well depends on the filter. (revisit one day to see how much filter is used)
		let source = umbExtensionsRegistry?.extensionsOfType<ManifestTypeName, ManifestType>(type);
		if (filter) {
			source = source.pipe(map((extensions) => extensions.filter(filter)));
		}
		this.observe(source, this.#gotManifests);
	}

	#gotManifests = (manifests: Array<ManifestType>) => {
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
		const existingIndex = this._permittedExts.indexOf(controller);
		if (isPermitted) {
			if (existingIndex === -1) {
				this._permittedExts.push(controller);
				hasChanged = true;
			}
		} else {
			if (existingIndex !== -1) {
				this._permittedExts.splice(existingIndex, 1);
				hasChanged = true;
			}
		}
		if (hasChanged) {
			this._permittedExts = this._permittedExts.filter((a) => a.permitted);
			console.log('this._permittedExts', this._permittedExts);
			this._permittedExts.sort((a, b) => b.weight - a.weight);
			this.#onChange(this._permittedExts, this as unknown as ControllerType);
			// Idea: could be abstracted into a requestChange method, so we can override it in a subclass.
		}
	};
}
