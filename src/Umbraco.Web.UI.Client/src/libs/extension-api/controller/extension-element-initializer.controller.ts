import { createExtensionElement } from '../functions/create-extension-element.function.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { isManifestElementableType } from '../type-guards/is-manifest-elementable-type.function.js';
import { ManifestCondition, ManifestWithDynamicConditions } from '../types.js';
import { UmbBaseExtensionInitializer } from './base-extension-initializer.controller.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * This Controller manages a single Extension and its Element.
 * When the extension is permitted to be used, its Element will be instantiated and available for the consumer.
 *
 * @example
* ```ts
* const controller = new UmbExtensionElementController(host, extensionRegistry, alias, (permitted, ctrl) => { console.log("Extension is permitted and this is the element: ", ctrl.component) }));
* ```
 * @export
 * @class UmbExtensionElementController
 */
export class UmbExtensionElementInitializer<
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions,
	ControllerType extends UmbExtensionElementInitializer<ManifestType, any> = any
> extends UmbBaseExtensionInitializer<ManifestType, ControllerType> {
	#defaultElement?: string;
	#component?: HTMLElement;

	/**
	 * The component that is created for this extension.
	 * @readonly
	 * @type {(HTMLElement | undefined)}
	 */
	public get component() {
		return this.#component;
	}

	/**
	 * The props that are passed to the component.
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
	#properties?: Record<string, unknown>;
	get properties() {
		return this.#properties;
	}
	set properties(newVal) {
		this.#properties = newVal;
		// TODO: we could optimize this so we only re-set the changed props.
		this.#assignProperties();
	}

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestCondition>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: ControllerType) => void,
		defaultElement?: string
	) {
		super(host, extensionRegistry, 'extElement_', alias, onPermissionChanged);
		this.#defaultElement = defaultElement;
		this._init();
	}

	#assignProperties = () => {
		if (!this.#component || !this.#properties) return;

		// TODO: we could optimize this so we only re-set the updated props.
		Object.keys(this.#properties).forEach((key) => {
			(this.#component as any)[key] = this.#properties![key];
		});
	};

	protected async _conditionsAreGood() {
		const manifest = this.manifest!; // In this case we are sure its not undefined.

		if (isManifestElementableType(manifest)) {
			const newComponent = await createExtensionElement(manifest, this.#defaultElement);
			if (!this._positive) {
				// We are not positive anymore, so we will back out of this creation.
				return false;
			}
			this.#component = newComponent;

		} else if (this.#defaultElement) {
			this.#component = document.createElement(this.#defaultElement);
		} else {
			this.#component = undefined;
			console.warn('Manifest did not provide any useful data for a web component to be created.')
		}
		if (this.#component) {
			this.#assignProperties();
			(this.#component as any).manifest = manifest;
			return true; // we will confirm we have a component and are still good to go.
		}

		return false; // we will reject the state, we have no component, we are not good to be shown.
	}

	protected async _conditionsAreBad() {
		// Destroy the element:
		if (this.#component) {
			if ('destroy' in this.#component) {
				(this.#component as unknown as { destroy: () => void }).destroy();
			}
			this.#component = undefined;
		}
	}
}
