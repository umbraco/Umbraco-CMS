import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import type {
	ManifestBase,
	UmbBaseExtensionInitializer,
	UmbExtensionRegistry,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export type PermittedControllerType<ControllerType extends { manifest: any }> = ControllerType & {
	manifest: Required<Pick<ControllerType, 'manifest'>>;
};

/**
 * This abstract Controller holds the core to manage a multiple Extensions.
 * When one or more extensions are permitted to be used, then the extender of this class can instantiate the relevant single extension initiator relevant for this type.
 * @abstract
 * @class UmbBaseExtensionsInitializer
 */
export abstract class UmbBaseExtensionsInitializer<
	ManifestTypes extends ManifestBase,
	ManifestTypeName extends string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbBaseExtensionInitializer<ManifestType> = UmbBaseExtensionInitializer<ManifestType>,
	MyPermittedControllerType extends ControllerType = PermittedControllerType<ControllerType>,
> extends UmbControllerBase {
	#promiseResolvers: Array<() => void> = [];
	#extensionRegistry: UmbExtensionRegistry<ManifestType>;
	#type: ManifestTypeName | Array<ManifestTypeName>;
	#filter: undefined | null | ((manifest: ManifestType) => boolean);
	#onChange?: (permittedManifests: Array<MyPermittedControllerType>) => void;
	protected _extensions: Array<ControllerType> = [];
	#permittedExts: Array<MyPermittedControllerType> = [];
	#exposedPermittedExts?: Array<MyPermittedControllerType>;
	#changeDebounce?: number;

	asPromise(): Promise<void> {
		return new Promise((resolve) => {
			if (this.#permittedExts.length > 0) {
				resolve();
			} else {
				this.#promiseResolvers.push(resolve);
			}
		});
	}

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestTypes>,
		type: ManifestTypeName | Array<ManifestTypeName>,
		filter: undefined | null | ((manifest: ManifestType) => boolean),
		onChange?: (permittedManifests: Array<MyPermittedControllerType>) => void,
		controllerAlias?: string,
	) {
		super(host, controllerAlias ?? 'extensionsInitializer_' + (Array.isArray(type) ? type.join('_') : type));
		this.#extensionRegistry = extensionRegistry;
		this.#type = type;
		this.#filter = filter;
		this.#onChange = onChange;
	}
	protected _init() {
		let source;

		if (Array.isArray(this.#type)) {
			if (this.#filter) {
				source = this.#extensionRegistry.byTypesAndFilter<ManifestType>(this.#type as string[], this.#filter);
			} else {
				source = this.#extensionRegistry.byTypes<ManifestType>(this.#type as string[]);
			}
		} else {
			if (this.#filter) {
				source = this.#extensionRegistry.byTypeAndFilter<ManifestTypeName, ManifestType>(
					this.#type as ManifestTypeName,
					this.#filter,
				);
			} else {
				source = this.#extensionRegistry.byType<ManifestTypeName, ManifestType>(this.#type as ManifestTypeName);
			}
		}
		this.observe(source, this.#gotManifests, '_observeManifests') as any;
	}

	#gotManifests = (manifests: Array<ManifestType>) => {
		if (!manifests) {
			// Clean up:
			this._extensions.forEach((controller) => {
				controller.destroy();
			});
			this._extensions.length = 0;
			// _permittedExts should have been cleared via the destroy callbacks.
			this.#permittedExts.length = 0;
			return;
		}

		// If we get no manifests and we have not exposed any extensions yet, then we should notify to let the listener know that we have our first response. [NL]
		if (manifests.length === 0 && this.#exposedPermittedExts === undefined) {
			this.#exposedPermittedExts = [];
			this.#onChange?.(this.#exposedPermittedExts);
		}

		// Clean up extensions that are no longer.
		this._extensions = this._extensions.filter((extension) => {
			if (!manifests.find((manifest) => manifest.alias === extension.alias)) {
				extension.destroy();
				// destroying the controller will, if permitted, make a last callback with isPermitted = false. This will also remove it from the _permittedExts array.
				return false;
			}
			return true;
		});

		manifests.forEach((manifest) => {
			const existing = this._extensions.find((x) => x.alias === manifest.alias);
			if (!existing) {
				this._extensions.push(this._createController(manifest));
			}
		});
	};

	protected abstract _createController(manifest: ManifestType): ControllerType;

	protected _extensionChanged = (isPermitted: boolean, controller: ControllerType) => {
		let hasChanged = false;

		// This might be called after this is destroyed, so we need to check if the _permittedExts is still available:
		const existingIndex = this.#permittedExts?.indexOf(controller as unknown as MyPermittedControllerType);
		if (isPermitted) {
			if (existingIndex === -1) {
				this.#permittedExts.push(controller as unknown as MyPermittedControllerType);
				hasChanged = true;
			}
		} else {
			if (existingIndex >= 0) {
				this.#permittedExts.splice(existingIndex, 1);
				hasChanged = true;
			}
		}

		if (hasChanged) {
			if (!this.#changeDebounce) {
				this.#changeDebounce = requestAnimationFrame(this.#notifyChange);
			}
		}
	};

	#notifyChange = () => {
		this.#changeDebounce = undefined;
		// This means that we have been destroyed:
		if (this.#permittedExts === undefined) return;

		// The final list of permitted extensions to be displayed, this will be stripped from extensions that are overwritten by another extension and sorted accordingly.
		this.#exposedPermittedExts = [...this.#permittedExts];

		// Removal of overwritten extensions:
		this.#permittedExts.forEach((extCtrl) => {
			// Check if it overwrites another extension:
			// if so, look up the extension it overwrites, and remove it from the list. and check that for if it overwrites another extension and so on.
			if (extCtrl.overwrites.length > 0) {
				extCtrl.overwrites.forEach((overwrite) => {
					this.#removeOverwrittenExtensions(this.#exposedPermittedExts!, overwrite);
				});
			}
		});

		// Sorting:
		this.#exposedPermittedExts.sort((a, b) => b.weight - a.weight);

		if (this.#exposedPermittedExts.length > 0) {
			this.#promiseResolvers.forEach((x) => x());
			this.#promiseResolvers = [];
		}

		// Collect change calls.
		this.#onChange?.(this.#exposedPermittedExts);
	};

	#removeOverwrittenExtensions(list: Array<MyPermittedControllerType>, alias: string) {
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

	override hostDisconnected(): void {
		super.hostDisconnected();
		if (this.#changeDebounce) {
			cancelAnimationFrame(this.#changeDebounce);
			this.#notifyChange();
		}
	}

	public override destroy() {
		// The this.#extensionRegistry is an indication of wether this is already destroyed.
		if (!this.#extensionRegistry) return;

		const oldPermittedExtsLength = this.#exposedPermittedExts?.length ?? 0;
		this._extensions.forEach((extension) => extension.destroy());
		(this._extensions as unknown) = undefined;
		(this.#permittedExts as unknown) = undefined;
		this.#exposedPermittedExts = undefined;
		if (this.#changeDebounce) {
			cancelAnimationFrame(this.#changeDebounce);
			this.#changeDebounce = undefined;
		}
		if (oldPermittedExtsLength > 0) {
			this.#onChange?.([]);
		}
		this.#promiseResolvers.length = 0;
		this.#filter = undefined;
		this.#onChange = undefined;
		(this.#extensionRegistry as unknown) = undefined;
		super.destroy();
	}
}
