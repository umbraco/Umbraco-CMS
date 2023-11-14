import type { ManifestTypeMap, SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import { type PermittedControllerType, UmbBaseExtensionsInitializer } from './base-extensions-initializer.controller.js';
import {
	type ManifestBase,
	UmbExtensionElementInitializer,
	type UmbExtensionRegistry,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 */
export class UmbExtensionsElementInitializer<
	ManifestTypes extends ManifestBase,
	ManifestTypeName extends keyof ManifestTypeMap<ManifestTypes> | string = ManifestTypes["type"],
	ManifestType extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, ManifestTypeName>,
	ControllerType extends UmbExtensionElementInitializer<ManifestType> = UmbExtensionElementInitializer<ManifestType>,
	MyPermittedControllerType extends ControllerType = PermittedControllerType<ControllerType>
> extends UmbBaseExtensionsInitializer<
	ManifestTypes,
	ManifestTypeName,
	ManifestType,
	ControllerType,
	MyPermittedControllerType
> {
	//
	#extensionRegistry;
	private _defaultElement?: string;
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
		defaultElement?: string
	) {
		super(host, extensionRegistry, type, filter, onChange);
		this.#extensionRegistry = extensionRegistry;
		this._defaultElement = defaultElement;
		this._init();
	}

	protected _createController(manifest: ManifestType) {
		const extController = new UmbExtensionElementInitializer<ManifestType>(
			this,
			this.#extensionRegistry,
			manifest.alias,
			this._extensionChanged,
			this._defaultElement
		) as ControllerType;

		extController.properties = this.#props;

		return extController;
	}
}
