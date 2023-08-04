import { UmbExtensionManifestController } from './extension-manifest-controller.js';
import { type PermittedControllerType, UmbBaseExtensionsController } from './base-extensions-controller.js';
import {
	ManifestBase,
	ManifestTypeMap,
	SpecificManifestTypeOrManifestBase,
	UmbExtensionRegistry,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export class UmbExtensionsManifestController<
	ManifestTypes extends ManifestBase,
	ManifestTypeName extends keyof ManifestTypeMap<ManifestTypes> | string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbExtensionManifestController<ManifestType> = UmbExtensionManifestController<ManifestType>,
	MyPermittedControllerType extends ControllerType = PermittedControllerType<ControllerType>
> extends UmbBaseExtensionsController<
	ManifestTypes,
	ManifestTypeName,
	ManifestType,
	ControllerType,
	MyPermittedControllerType
> {
	//
	#extensionRegistry: UmbExtensionRegistry<ManifestTypes>;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestTypes>,
		type: ManifestTypeName | Array<ManifestTypeName>,
		filter: null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<MyPermittedControllerType>, controller: MyPermittedControllerType) => void
	) {
		super(host, extensionRegistry, type, filter, onChange);
		this.#extensionRegistry = extensionRegistry;
		this._init();
	}

	protected _createController(manifest: ManifestType) {
		return new UmbExtensionManifestController<ManifestType>(
			this,
			this.#extensionRegistry,
			manifest.alias,
			this._extensionChanged
		) as ControllerType;
	}
}
