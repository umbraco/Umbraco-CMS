import type { ManifestBase } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import { UmbExtensionElementInitializer } from './extension-element-initializer.controller.js';
import {
	type PermittedControllerType,
	UmbBaseExtensionsInitializer,
	type UmbBaseExtensionsInitializerArgs,
} from './base-extensions-initializer.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbExtensionsElementInitializerArgs extends UmbBaseExtensionsInitializerArgs {}

/**
 */
export class UmbExtensionsElementInitializer<
	ManifestTypes extends ManifestBase = ManifestBase,
	ManifestTypeName extends string = string,
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbExtensionElementInitializer<ManifestType> = UmbExtensionElementInitializer<ManifestType>,
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
		filter: undefined | null | ((manifest: ManifestType) => boolean),
		onChange: (permittedManifests: Array<MyPermittedControllerType>) => void,
		controllerAlias?: string,
		defaultElement?: string,
		args?: UmbExtensionsElementInitializerArgs,
	) {
		super(host, extensionRegistry, type, filter, onChange, controllerAlias, args);
		this.#extensionRegistry = extensionRegistry;
		this.#defaultElement = defaultElement;
		this._init();
	}

	protected _createController(manifest: ManifestType) {
		const extController = new UmbExtensionElementInitializer<ManifestType>(
			this,
			this.#extensionRegistry,
			manifest.alias,
			this._extensionChanged,
			this.#defaultElement,
		) as ControllerType;

		extController.properties = this.#props;

		return extController;
	}

	public override destroy(): void {
		super.destroy();
		this.#props = undefined;
		(this.#extensionRegistry as unknown) = undefined;
	}
}
