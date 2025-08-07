import { umbExtensionsRegistry } from './registry.js';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';

export abstract class UmbExtensionElementAndApiSlotElementBase<
	ManifestType extends ManifestElementAndApi,
> extends UmbLitElement {
	@property({ type: String, reflect: true })
	get alias() {
		return this.#alias;
	}
	set alias(newVal) {
		this.#alias = newVal;
		this.#observeManifest();
	}
	#alias?: string;

	@property({ type: Object, attribute: false })
	set props(newVal: Record<string, unknown> | undefined) {
		// TODO, compare changes since last time. only reset the ones that changed. This might be better done by the controller is self:
		this.#props = newVal;
		if (this.#extensionController) {
			this.#extensionController.elementProps = newVal;
		}
	}
	get props() {
		return this.#props;
	}

	#props?: Record<string, unknown> = {};

	#extensionController?: UmbExtensionElementAndApiInitializer<ManifestType>;

	@state()
	protected _api: ManifestType['API_TYPE'] | undefined;

	@state()
	protected _element: ManifestType['ELEMENT_TYPE'] | undefined;

	abstract getExtensionType(): string;
	abstract getDefaultElementName(): string;

	#observeManifest() {
		if (!this.alias) return;

		this.#extensionController = new UmbExtensionElementAndApiInitializer<ManifestType>(
			this,
			umbExtensionsRegistry,
			this.alias,
			[this],
			this.#extensionChanged,
			this.getDefaultElementName(),
		);
		this.#extensionController.elementProps = this.props;
	}

	#extensionChanged = (isPermitted: boolean, controller: UmbExtensionElementAndApiInitializer<ManifestType>) => {
		this.apiChanged(isPermitted ? controller.api : undefined);
		this.elementChanged(isPermitted ? controller.component : undefined);
	};

	/**
	 * Called when the API is changed.
	 * @protected
	 * @param {(ManifestType['API_TYPE'] | undefined)} api
	 * @memberof UmbExtensionElementAndApiSlotElementBase
	 */
	protected apiChanged(api: ManifestType['API_TYPE'] | undefined) {
		this._api = api;
	}

	/**
	 * Called when the element is changed.
	 * @protected
	 * @param {(ManifestType['ELEMENT_TYPE'] | undefined)} element
	 * @memberof UmbExtensionElementAndApiSlotElementBase
	 */
	protected elementChanged(element: ManifestType['ELEMENT_TYPE'] | undefined) {
		this._element = element;
		this.requestUpdate('_element');
	}

	/**
	 * Render the element.
	 * @returns {*}
	 * @memberof UmbExtensionElementAndApiSlotElementBase
	 */
	override render() {
		return this._element;
	}

	/**
	 * Disable the Shadow DOM for this element. This is needed because this is a wrapper element and should not stop the event propagation.
	 */
	protected override createRenderRoot() {
		return this;
	}
}
