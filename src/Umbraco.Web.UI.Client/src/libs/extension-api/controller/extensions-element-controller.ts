import { UmbBaseExtensionsController } from './base-extensions-controller.js';
import {
	type ManifestBase,
	type ManifestTypeMap,
	type SpecificManifestTypeOrManifestBase,
	UmbExtensionElementController,
} from '@umbraco-cms/backoffice/extension-api';
import { type ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export class UmbExtensionsElementController<
	ManifestTypeName extends keyof ManifestTypeMap<ManifestTypes> | string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbExtensionElementController<ManifestType> = UmbExtensionElementController<ManifestType>
> extends UmbBaseExtensionsController<ManifestTypeName, ManifestType, ControllerType> {
	private _defaultElement?: string;

	constructor(
		host: UmbControllerHost,
		type: ManifestTypeName,
		filter: null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<ControllerType>, controller: ControllerType) => void,
		defaultElement?: string
	) {
		super(host, type, filter, onChange);
		this._defaultElement = defaultElement;
	}

	protected _createController(manifest: ManifestType) {
		return new UmbExtensionElementController<ManifestType>(
			this,
			umbExtensionsRegistry,
			manifest.alias,
			this._extensionChanged,
			this._defaultElement
		) as ControllerType;
	}
}
