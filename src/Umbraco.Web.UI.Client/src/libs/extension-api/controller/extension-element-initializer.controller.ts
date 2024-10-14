import { createExtensionElement } from '../functions/create-extension-element.function.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { ManifestCondition, ManifestElement, ManifestWithDynamicConditions } from '../types/index.js';
import { UmbBaseExtensionInitializer } from './base-extension-initializer.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * This Controller manages a single Extension and its Element.
 * When the extension is permitted to be used, its Element will be instantiated and available for the consumer.
 * @example
 * ```ts
 * const controller = new UmbExtensionElementController(host, extensionRegistry, alias, (permitted, ctrl) => { console.log("Extension is permitted and this is the element: ", ctrl.component) }));
 * ```
 * @class UmbExtensionElementController
 */
export class UmbExtensionElementInitializer<
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions,
	ControllerType extends UmbExtensionElementInitializer<ManifestType, any> = any,
	ExtensionInterface extends ManifestElement = ManifestType extends ManifestElement ? ManifestType : never,
	ExtensionElementInterface extends HTMLElement | undefined = ExtensionInterface['ELEMENT_TYPE'],
> extends UmbBaseExtensionInitializer<ManifestType, ControllerType> {
	#defaultElement?: string;
	#component?: ExtensionElementInterface;

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
		defaultElement?: string,
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

		const newComponent = await createExtensionElement(manifest, this.#defaultElement);
		if (!this._isConditionsPositive) {
			// We are not positive anymore, so we will back out of this creation.
			return false;
		}
		this.#component = newComponent as ExtensionElementInterface;
		if (this.#component) {
			this.#assignProperties();
			(this.#component as any).manifest = manifest;
			return true; // we will confirm we have a component and are still good to go.
		} else {
			console.warn('Manifest did not provide any useful data for a web component to be created.');
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

	public override destroy(): void {
		super.destroy();
		this.#properties = undefined;
	}
}
