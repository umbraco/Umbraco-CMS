import { UmbManifestExtensionController } from './manifest-extension-controller.js';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { ManifestBase, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export class UmbManifestsExtensionController<
	ManifestType extends ManifestWithDynamicConditions
> extends UmbBaseController {
	#onChange: (
		permittedManifests: Array<UmbManifestExtensionController<ManifestType>>,
		controller: UmbManifestsExtensionController<ManifestType>
	) => void;
	private _extensions: Array<UmbManifestExtensionController<ManifestType>> = [];
	private _permittedExts: Array<UmbManifestExtensionController<ManifestType>> = [];

	constructor(
		host: UmbControllerHost,
		type: string,
		filter: null | ((manifest: ManifestBase) => boolean),
		onChange: (
			permittedManifests: Array<UmbManifestExtensionController<ManifestType>>,
			controller: UmbManifestsExtensionController<ManifestType>
		) => void
	) {
		super(host);
		this.#onChange = onChange;

		// TODO: This could be optimized by just getting the aliases, well depends on the filter. (revisit one day to see how much filter is used)
		let source = umbExtensionsRegistry?.extensionsOfType(type);
		if (filter) {
			source = source.pipe(map((extensions) => extensions.filter(filter)));
		}
		this.observe(source, this.#gotManifests);
	}

	#gotManifests = (manifests: Array<ManifestBase>) => {
		const oldLength = this._extensions.length;

		// Clean up extensions that are no longer.
		this._extensions = this._extensions.filter((controller) => {
			if (!manifests.find((manifest) => manifest.alias === controller.alias)) {
				controller.destroy();
				// destroying the controller will, if permitted, make a last callback with isPermitted = false.
				return false;
			}
			return true;
		});

		// If not the same length, then we need to make an update already, so removed extensions can be removed from the DOM immediately.
		if (this._extensions.length !== oldLength) {
			this.#onChange(this._permittedExts, this);
			// Idea: could be abstracted into a requestChange method, so we can override it in a subclass.
		}

		// ---------------------------------------------------------------
		// May change this into a Extensions Manager Controller???
		// ---------------------------------------------------------------

		manifests.forEach((manifest) => {
			const existing = this._extensions.find((x) => x.alias === manifest.alias);
			if (!existing) {
				// Idea: could be abstracted into a createController method, so we can override it in a subclass.
				// (This should be enough to be able to create a element extension controller instead.)
				const controller = new UmbManifestExtensionController<ManifestType>(
					this,
					umbExtensionsRegistry,
					manifest.alias,
					this.#extensionChanged
				);
				this._extensions.push(controller);
			}
		});
	};

	#extensionChanged = (isPermitted: boolean, controller: UmbManifestExtensionController<ManifestType>) => {
		const oldValue = this._permittedExts;
		const oldLength = oldValue.length;
		const existingIndex = this._permittedExts.indexOf(controller);
		if (isPermitted) {
			if (existingIndex === -1) {
				this._permittedExts.push(controller);
			}
		} else {
			if (existingIndex !== -1) {
				this._permittedExts.splice(existingIndex, 1);
			}
		}
		if (oldLength !== this._permittedExts.length) {
			this._permittedExts.sort((a, b) => b.weight - a.weight);
			this.#onChange(this._permittedExts, this);
			// Idea: could be abstracted into a requestChange method, so we can override it in a subclass.
		}
	};
}
