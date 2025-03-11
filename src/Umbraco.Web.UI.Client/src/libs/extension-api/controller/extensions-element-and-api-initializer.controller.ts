import type { ApiLoaderProperty, ManifestBase } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import type { UmbApiConstructorArgumentsMethodType, UmbBaseExtensionsInitializerArgs } from '../index.js';
import { UmbExtensionElementAndApiInitializer } from './extension-element-and-api-initializer.controller.js';
import {
	type PermittedControllerType,
	UmbBaseExtensionsInitializer,
} from './base-extensions-initializer.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbExtensionsElementAndApiInitializerArgs extends UmbBaseExtensionsInitializerArgs {}

/**
 */
export class UmbExtensionsElementAndApiInitializer<
	ManifestTypes extends ManifestBase = ManifestBase,
	ManifestTypeName extends string = string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends
		UmbExtensionElementAndApiInitializer<ManifestType> = UmbExtensionElementAndApiInitializer<ManifestType>,
	MyPermittedControllerType extends ControllerType = PermittedControllerType<ControllerType>,
> extends UmbBaseExtensionsInitializer<
	ManifestTypes,
	ManifestTypeName,
	ManifestType,
	ControllerType,
	MyPermittedControllerType
> {
	//
	#extensionRegistry;
	#defaultElement?: string;
	#defaultApi?: ApiLoaderProperty;
	#constructorArgs: Array<unknown> | UmbApiConstructorArgumentsMethodType<ManifestType> | undefined;
	#elProps?: Record<string, unknown>;
	#apiProps?: Record<string, unknown>;

	public get elementProperties() {
		return this.#elProps;
	}
	public set elementProperties(props: Record<string, unknown> | undefined) {
		this.#elProps = props;
		this._extensions.forEach((controller) => {
			controller.elementProps = props;
		});
	}

	public get apiProperties() {
		return this.#apiProps;
	}
	public set apiProperties(props: Record<string, unknown> | undefined) {
		this.#apiProps = props;
		this._extensions.forEach((controller) => {
			controller.apiProps = props;
		});
	}

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestTypes>,
		type: ManifestTypeName | Array<ManifestTypeName>,
		constructorArguments: Array<unknown> | UmbApiConstructorArgumentsMethodType<ManifestType> | undefined,
		filter: undefined | null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<MyPermittedControllerType>) => void,
		controllerAlias?: string,
		defaultElement?: string,
		defaultApi?: ApiLoaderProperty,
		args?: UmbExtensionsElementAndApiInitializerArgs,
	) {
		super(host, extensionRegistry, type, filter, onChange, controllerAlias, args);
		this.#extensionRegistry = extensionRegistry;
		this.#constructorArgs = constructorArguments;
		this.#defaultElement = defaultElement;
		this.#defaultApi = defaultApi;
		this._init();
	}

	protected _createController(manifest: ManifestType) {
		const extController = new UmbExtensionElementAndApiInitializer<ManifestType>(
			this,
			this.#extensionRegistry,
			manifest.alias,
			this.#constructorArgs,
			this._extensionChanged,
			this.#defaultElement,
			this.#defaultApi as any,
		) as ControllerType;

		extController.elementProps = this.#elProps;
		extController.apiProps = this.#apiProps;

		return extController;
	}

	public override destroy(): void {
		super.destroy();
		this.#constructorArgs = undefined;
		this.#elProps = undefined;
		this.#apiProps = undefined;
		(this.#extensionRegistry as unknown) = undefined;
	}
}
