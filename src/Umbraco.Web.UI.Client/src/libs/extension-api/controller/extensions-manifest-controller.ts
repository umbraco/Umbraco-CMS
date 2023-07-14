import { UmbExtensionManifestController } from './extension-manifest-controller.js';
import { UmbBaseExtensionsController } from './base-extensions-controller.js';
import { ManifestTypeMap, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export class UmbExtensionsManifestController<
	ManifestAlias extends keyof ManifestTypeMap<ManifestTypes> | string,
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions,
	ControllerType extends UmbExtensionManifestController<ManifestType> = UmbExtensionManifestController<ManifestType>
> extends UmbBaseExtensionsController<ManifestAlias, ManifestType, ControllerType> {
	constructor(
		host: UmbControllerHost,
		type: ManifestAlias,
		filter: null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<ControllerType>, controller: ControllerType) => void
	) {
		super(host, type, filter, onChange);
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
