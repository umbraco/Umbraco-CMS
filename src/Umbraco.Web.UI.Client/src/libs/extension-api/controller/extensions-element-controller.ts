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
	ManifestAlias extends keyof ManifestTypeMap<ManifestTypes> | string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestAlias>,
	ControllerType extends UmbExtensionElementController<ManifestType> = UmbExtensionElementController<ManifestType>
> extends UmbBaseExtensionsController<ManifestAlias, ManifestType, ControllerType> {
	private _defaultElement?: string;
	private _type: ManifestAlias;

	constructor(
		host: UmbControllerHost,
		type: ManifestAlias,
		filter: null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<ControllerType>, controller: ControllerType) => void,
		defaultElement?: string
	) {
		super(host, type, filter, onChange);
		this._defaultElement = defaultElement;
		this._type = type;
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
