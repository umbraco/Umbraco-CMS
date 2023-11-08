import { createExtensionApi } from '../functions/create-extension-api.function.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { isManifestApiType } from '../type-guards/is-manifest-apiable-type.function.js';
import { ExtensionApi, ManifestApi, ManifestCondition, ManifestWithDynamicConditions } from '../types.js';
import { UmbBaseExtensionController } from './base-extension-controller.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbExtensionApiController<
	ManifestType extends (ManifestWithDynamicConditions & ManifestApi<ApiType>) = (ManifestWithDynamicConditions & ManifestApi<any>),
	ControllerType extends UmbExtensionApiController<ManifestType> = any,
	ApiType extends ExtensionApi = ExtensionApi,
> extends UmbBaseExtensionController<ManifestType, ControllerType> {

	_api?: ApiType;

	/**
	 * The api that is created for this extension.
	 * @readonly
	 * @type {(class | undefined)}
	 */
	public get api() {
		return this._api;
	}

	/**
	 * The arguments passed to the class constructor.
	 * @type {Array<any>}
	 * @memberof UmbApiExtensionController
	 * @example
	 * ```ts
	 * const controller = new UmbApiExtensionController(host, extensionRegistry, alias, onPermissionChanged);
	 * controller.props = { foo: 'bar' };
	 * ```
	 * Is equivalent to:
	 * ```ts
	 * controller.component.foo = 'bar';
	 * ```
	 */
	#constructorArguments?: Array<unknown>;
	get constructorArguments() {
		return this.#constructorArguments;
	}
	set constructorArguments(newVal) {
		this.#constructorArguments = newVal;
		if(this._api) {
			console.warn('Constructor Arguments was set after api class has been constructed')
		}
	}

	/**
	 * The props that are passed to the class.
	 * @type {Record<string, any>}
	 * @memberof UmbElementExtensionController
	 * @example
	 * ```ts
	 * const controller = new UmbElementExtensionController(host, extensionRegistry, alias, onPermissionChanged);
	 * controller.props = { foo: 'bar' };
	 * ```
	 * Is equivalent to:
	 * ```ts
	 * controller.component.foo = 'bar';
	 * ```
	 */
	/*
	#properties?: Record<string, unknown>;
	get properties() {
		return this.#properties;
	}
	set properties(newVal) {
		this.#properties = newVal;
		// TODO: we could optimize this so we only re-set the changed props.
		this.#assignProperties();
	}
	*/

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestCondition>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: ControllerType) => void
	) {
		super(host, extensionRegistry, alias, onPermissionChanged);
		this._init();
	}

	/*
	#assignProperties = () => {
		if (!this._api || !this.#properties) return;

		// TODO: we could optimize this so we only re-set the updated props.
		Object.keys(this.#properties).forEach((key) => {
			(this._api as any)[key] = this.#properties![key];
		});
	};
	*/

	protected async _conditionsAreGood() {
		const manifest = this.manifest!; // In this case we are sure its not undefined.

		if (isManifestApiType(manifest)) {
			const newApi = await createExtensionApi<ApiType>(manifest, this.#constructorArguments);
			if (!this._positive) {
				// We are not positive anymore, so we will back out of this creation.
				return false;
			}
			this._api = newApi;

		} else {
			this._api = undefined;
			console.warn('Manifest did not provide any useful data for a api class to construct.')
		}
		if (this._api) {
			//this.#assignProperties();
			return true; // we will confirm we have a component and are still good to go.
		}

		return false; // we will reject the state, we have no component, we are not good to be shown.
	}

	protected async _conditionsAreBad() {
		// Destroy the element:
		if (this._api) {
			if ('destroy' in this._api) {
				(this._api as unknown as { destroy: () => void }).destroy();
			}
			this._api = undefined;
		}
	}
}
