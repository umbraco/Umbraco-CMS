import { type PermittedControllerType, UmbBaseExtensionsController } from './base-extensions-controller.js';
import { UmbExtensionApiController } from './extension-api-controller.js';
import {
	type ManifestTypeMap,
	type SpecificManifestTypeOrManifestBase,
	type UmbExtensionRegistry,
	ManifestApi,
	ManifestBase,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export class UmbExtensionsApiController<
	ManifestTypes extends ManifestApi,
	ManifestTypeName extends keyof ManifestTypeMap<ManifestTypes> | string = string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ManifestTypeAsApi extends ManifestApi = ManifestType extends ManifestApi ? ManifestType : never,
	ControllerType extends UmbExtensionApiController<ManifestTypeAsApi> = UmbExtensionApiController<ManifestTypeAsApi>,
	MyPermittedControllerType extends ControllerType = PermittedControllerType<ControllerType>
> extends UmbBaseExtensionsController<
	ManifestTypes,
	ManifestTypeName,
	ManifestTypeAsApi,
	ControllerType,
	MyPermittedControllerType
> {
	//
	#extensionRegistry;

	/*
	#props?: Record<string, unknown>;

	public get properties() {
		return this.#props;
	}
	public set properties(props: Record<string, unknown> | undefined) {
		this.#props = props;
		this._extensions.forEach((controller) => {
			controller.properties = props;
		});
	}
	*/

	#constructorArgs: Array<unknown> | undefined;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestTypes>,
		type: ManifestTypeName | Array<ManifestTypeName>,
		constructorArguments: Array<unknown> | undefined,
		filter: undefined | null | ((manifest: ManifestTypeAsApi) => boolean),
		onChange: (permittedManifests: Array<MyPermittedControllerType>, controller: MyPermittedControllerType) => void
	) {
		super(host, extensionRegistry, type, filter, onChange);
		this.#extensionRegistry = extensionRegistry;
		this.#constructorArgs = constructorArguments;
		this._init();
	}

	protected _createController(manifest: ManifestTypeAsApi) {
		const extController = new UmbExtensionApiController<ManifestTypeAsApi>(
			this,
			this.#extensionRegistry,
			manifest.alias,
			this.#constructorArgs,
			this._extensionChanged
		) as ControllerType;

		return extController;
	}
}
