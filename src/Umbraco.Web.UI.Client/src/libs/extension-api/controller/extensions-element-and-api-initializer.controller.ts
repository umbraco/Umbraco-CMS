import type { ManifestBase } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import { UmbExtensionElementAndApiInitializer } from './extension-element-and-api-initializer.controller.js';
import {
	type PermittedControllerType,
	UmbBaseExtensionsInitializer,
} from './base-extensions-initializer.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
	#constructorArgs: Array<unknown> | undefined;
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

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestTypes>,
		type: ManifestTypeName | Array<ManifestTypeName>,
		constructorArguments: Array<unknown> | undefined,
		filter: undefined | null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<MyPermittedControllerType>) => void,
		controllerAlias?: string,
		defaultElement?: string,
	) {
		super(host, extensionRegistry, type, filter, onChange, controllerAlias);
		this.#extensionRegistry = extensionRegistry;
		this.#constructorArgs = constructorArguments;
		this.#defaultElement = defaultElement;
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
		) as ControllerType;

		extController.properties = this.#props;

		return extController;
	}

	public destroy(): void {
		super.destroy();
		this.#constructorArgs = undefined;
		this.#props = undefined;
		(this.#extensionRegistry as any) = undefined;
	}
}
