import { UmbExtensionController } from './extension-controller.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { createExtensionElement, isManifestElementableType } from '@umbraco-cms/backoffice/extension-api';
import { property } from '@umbraco-cms/backoffice/external/lit';

export class UmbElementExtensionController extends UmbExtensionController {
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
	 * const controller = new UmbElementExtensionController(host, alias, onPermissionChanged);
	 * controller.props = { foo: 'bar' };
	 * ```
	 * Is equivalent to:
	 * ```ts
	 * controller.component.foo = 'bar';
	 * ```
	 */
	#props?: Record<string, any> = {};
	@property({ type: Object, attribute: false })
	get props() {
		return this.#props;
	}
	set props(newVal) {
		this.#props = newVal;
		// TODO: we could optimize this so we only re-set the changed props.
		this.#assignProps();
	}

	constructor(host: UmbControllerHost, alias: string, onPermissionChanged: () => void, defaultElement?: string) {
		super(host, alias, onPermissionChanged);
		this.#defaultElement = defaultElement;
	}

	#assignProps = () => {
		if (!this.#component || !this.#props) return;

		Object.keys(this.#props).forEach((key) => {
			(this.#component as any)[key] = this.#props?.[key];
		});
	};

	protected async _conditionsAreGood() {
		// Create the element/class.

		const manifest = this.manifest!; // In this case we are sure its not undefined.

		if (isManifestElementableType(manifest)) {
			this.#component = await createExtensionElement(manifest);
		} else if (this.#defaultElement) {
			this.#component = document.createElement(this.#defaultElement);
		} else {
			this.#component = undefined;
			// TODO: Lets make an console.error in this case? we could not initialize any component based on this manifest.
		}
		if (this.#component) {
			this.#assignProps();
			(this.#component as any).manifest = manifest;
			return true; // we will confirm we have a component and are still good to go.
		}

		return false; // we will reject the state, we have no component, we are not good to be shown.
	}

	protected async _conditionsAreBad() {
		// Destroy the element/class.
	}
}
