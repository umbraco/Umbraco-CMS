import { UmbExtensionManifestController } from './extension-manifest-controller.js';
import { UmbBaseExtensionsController } from './base-extensions-controller.js';
import {
	ManifestBase,
	ManifestTypeMap,
	SpecificManifestTypeOrManifestBase,
} from '@umbraco-cms/backoffice/extension-api';
import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export class UmbExtensionsManifestController<
	ManifestTypeName extends keyof ManifestTypeMap<ManifestTypes> | string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbExtensionManifestController<ManifestType> = UmbExtensionManifestController<ManifestType>,
	PermittedControllerType extends ControllerType = ControllerType & {
		manifest: Required<Pick<ControllerType, 'manifest'>>;
	}
> extends UmbBaseExtensionsController<ManifestTypeName, ManifestType, ControllerType, PermittedControllerType> {
	constructor(
		host: UmbControllerHost,
		type: ManifestTypeName,
		filter: null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<PermittedControllerType>, controller: PermittedControllerType) => void
	) {
		super(host, umbExtensionsRegistry, type, filter, onChange);
	}

	protected _createController(manifest: ManifestType) {
		return new UmbExtensionManifestController<ManifestType>(
			this,
			umbExtensionsRegistry,
			manifest.alias,
			this._extensionChanged
		) as ControllerType;
	}
}
