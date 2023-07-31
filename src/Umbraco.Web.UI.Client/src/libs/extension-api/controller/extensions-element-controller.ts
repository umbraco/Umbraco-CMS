import { type PermittedControllerType, UmbBaseExtensionsController } from './base-extensions-controller.js';
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
	ManifestTypeName extends keyof ManifestTypeMap<ManifestTypes> | string = string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbExtensionElementController<ManifestType> = UmbExtensionElementController<ManifestType>,
	MyPermittedControllerType extends ControllerType = PermittedControllerType<ControllerType>
> extends UmbBaseExtensionsController<ManifestTypeName, ManifestType, ControllerType, MyPermittedControllerType> {
	// Properties:
	private _defaultElement?: string;

	constructor(
		host: UmbControllerHost,
		type: ManifestTypeName,
		filter: undefined | null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<MyPermittedControllerType>, controller: MyPermittedControllerType) => void,
		defaultElement?: string
	) {
		super(host, umbExtensionsRegistry, type, filter, onChange);
		this._defaultElement = defaultElement;
		this._init();
	}

	protected _createController(manifest: ManifestType) {
		if (manifest.type === 'menuItem') {
			console.log('create for', manifest);
		}
		return new UmbExtensionElementController<ManifestType>(
			this,
			umbExtensionsRegistry,
			manifest.alias,
			this._extensionChanged,
			this._defaultElement
		) as ControllerType;
	}
}
