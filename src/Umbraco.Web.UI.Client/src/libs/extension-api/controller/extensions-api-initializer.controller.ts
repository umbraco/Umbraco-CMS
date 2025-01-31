import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import {
	type PermittedControllerType,
	UmbBaseExtensionsInitializer,
} from './base-extensions-initializer.controller.js';
import { UmbExtensionApiInitializer } from './extension-api-initializer.controller.js';
import type {
	ManifestApi,
	ManifestBase,
	UmbApiConstructorArgumentsMethodType,
	UmbBaseExtensionsInitializerArgs,
	UmbExtensionRegistry,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbExtensionsApiInitializerArgs extends UmbBaseExtensionsInitializerArgs {}

/**
 * This Controller manages a set of Extensions and their Manifest.
 * When one or more extensions is permitted to be used, the callback gives all permitted extensions and their manifest.
 * @example
 * ```ts
TODO: Correct this, start using builder pattern:
 * const controller = new UmbExtensionsApiInitializer(host, extensionRegistry, type, ['constructor argument 1', 'constructor argument '], filter?, (permitted, ctrl) => { console.log("Extension is permitted and this is the manifest: ", ctrl.manifest) }));
 * ```
 * @class UmbExtensionsApiInitializer
 */
export class UmbExtensionsApiInitializer<
	ManifestTypes extends ManifestApi,
	ManifestTypeName extends string = string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ManifestTypeAsApi extends ManifestApi = ManifestType extends ManifestApi ? ManifestType : never,
	ControllerType extends UmbExtensionApiInitializer<ManifestTypeAsApi> = UmbExtensionApiInitializer<ManifestTypeAsApi>,
	MyPermittedControllerType extends ControllerType = PermittedControllerType<ControllerType>,
> extends UmbBaseExtensionsInitializer<
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

	#constructorArgs: Array<unknown> | UmbApiConstructorArgumentsMethodType<ManifestTypeAsApi> | undefined;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestTypes>,
		type: ManifestTypeName | Array<ManifestTypeName>,
		constructorArguments: Array<unknown> | UmbApiConstructorArgumentsMethodType<ManifestTypeAsApi> | undefined,
		filter?: undefined | null | ((manifest: ManifestTypeAsApi) => boolean),
		onChange?: (permittedManifests: Array<MyPermittedControllerType>) => void,
		controllerAlias?: string,
		args?: UmbExtensionsApiInitializerArgs,
	) {
		super(host, extensionRegistry, type, filter, onChange, controllerAlias, args);
		this.#extensionRegistry = extensionRegistry;
		this.#constructorArgs = constructorArguments;
		this._init();
	}

	protected _createController(manifest: ManifestTypeAsApi) {
		const extController = new UmbExtensionApiInitializer<ManifestTypeAsApi>(
			this,
			this.#extensionRegistry,
			manifest.alias,
			this.#constructorArgs,
			this._extensionChanged,
		) as ControllerType;

		return extController;
	}

	public override destroy(): void {
		super.destroy();
		this.#constructorArgs = undefined;
		(this.#extensionRegistry as unknown) = undefined;
	}
}
