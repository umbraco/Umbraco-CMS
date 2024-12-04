import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import { UmbExtensionManifestInitializer } from './extension-manifest-initializer.controller.js';
import {
	type PermittedControllerType,
	UmbBaseExtensionsInitializer,
} from './base-extensions-initializer.controller.js';
import type { ManifestBase, UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export class UmbExtensionsManifestInitializer<
	ManifestTypes extends ManifestBase,
	ManifestTypeName extends string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbExtensionManifestInitializer<ManifestType> = UmbExtensionManifestInitializer<ManifestType>,
	MyPermittedControllerType extends ControllerType = PermittedControllerType<ControllerType>,
> extends UmbBaseExtensionsInitializer<
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
		onChange: (permittedManifests: Array<MyPermittedControllerType>) => void,
		controllerAlias?: string,
	) {
		super(host, extensionRegistry, type, filter, onChange, controllerAlias);
		this.#extensionRegistry = extensionRegistry;
		this._init();
	}

	protected _createController(manifest: ManifestType) {
		return new UmbExtensionManifestInitializer<ManifestType>(
			this,
			this.#extensionRegistry,
			manifest.alias,
			this._extensionChanged,
		) as ControllerType;
	}

	public override destroy(): void {
		super.destroy();
		(this.#extensionRegistry as unknown) = undefined;
	}
}
