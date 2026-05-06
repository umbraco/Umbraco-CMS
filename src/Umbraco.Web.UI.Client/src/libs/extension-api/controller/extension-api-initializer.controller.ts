import { createExtensionApi } from '../functions/create-extension-api.function.js';
import type { UmbApiConstructorArgumentsMethodType } from '../functions/types.js';
import type { UmbApi } from '../models/api.interface.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { ManifestApi, ManifestCondition } from '../types/index.js';
import { UmbBaseExtensionInitializer } from './base-extension-initializer.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * This Controller manages a single Extension and its API instance.
 * When the extension is permitted to be used, its API will be instantiated and available for the consumer.
 * @example
 * ```ts
 * const controller = new UmbExtensionApiController(host, extensionRegistry, alias, [], (permitted, ctrl) => { ctrl.api.helloWorld() }));
 * ```
 * @class UmbExtensionApiController
 */
export class UmbExtensionApiInitializer<
	ManifestType extends ManifestApi = ManifestApi,
	ControllerType extends UmbExtensionApiInitializer<ManifestType, any> = any,
	ExtensionApiInterface extends UmbApi = ManifestType extends ManifestApi
		? NonNullable<ManifestType['API_TYPE']>
		: UmbApi,
> extends UmbBaseExtensionInitializer<ManifestType, ControllerType> {
	#api?: ExtensionApiInterface;
	#constructorArguments?: Array<unknown> | UmbApiConstructorArgumentsMethodType<ManifestType>;

	/**
	 * The api that is created for this extension.
	 * @readonly
	 * @type {(class | undefined)}
	 */
	public get api() {
		return this.#api;
	}

	/**
	 * The props that are passed to the class.
	 * @type {Record<string, any>}
	 * @memberof UmbExtensionApiController
	 * @example
	 * ```ts
	 * const controller = new UmbExtensionApiController(host, extensionRegistry, alias, [], onPermissionChanged);
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
		constructorArguments: Array<unknown> | UmbApiConstructorArgumentsMethodType<ManifestType> | undefined,
		onPermissionChanged?: (isPermitted: boolean, controller: ControllerType) => void,
	) {
		super(host, extensionRegistry, 'extApi_', alias, onPermissionChanged);
		this.#constructorArguments = constructorArguments;
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

		const newApi = await createExtensionApi<ExtensionApiInterface>(
			this._host,
			manifest as unknown as ManifestApi<ExtensionApiInterface>,
			this.#constructorArguments as any,
		);
		if (!this._isConditionsPositive) {
			// We are not positive anymore, so we will back out of this creation.
			return false;
		}
		this.#api = newApi;

		if (this.#api) {
			(this.#api as any).manifest = manifest;
			//this.#assignProperties();
			return true; // we will confirm we have a component and are still good to go.
		}

		console.warn('Manifest did not provide any useful data for a api class to construct.');
		return false; // we will reject the state, we have no component, we are not good to be shown.
	}

	protected async _conditionsAreBad() {
		// Destroy the api:
		if (this.#api) {
			if ('destroy' in this.#api) {
				(this.#api as unknown as { destroy: () => void }).destroy();
			}
			this.#api = undefined;
		}
	}

	public override destroy(): void {
		super.destroy();
		this.#constructorArguments = undefined;
	}
}
