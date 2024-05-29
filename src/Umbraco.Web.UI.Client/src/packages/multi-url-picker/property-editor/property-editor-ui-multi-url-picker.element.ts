import type { UmbLinkPickerLink } from '../link-picker-modal/types.js';
import type { UmbMultiUrlPickerElement } from '../multi-url-picker/multi-url-picker.element.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

// import of local component
import '../multi-url-picker/multi-url-picker.element.js';

/**
 * @element umb-property-editor-ui-multi-url-picker
 */
@customElement('umb-property-editor-ui-multi-url-picker')
export class UmbPropertyEditorUIMultiUrlPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Array })
	value: Array<UmbLinkPickerLink> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._hideAnchor = Boolean(config.getValueByAlias('hideAnchor')) ?? false;
		this._min = this.#parseInt(config.getValueByAlias('minNumber'), 0);
		this._max = this.#parseInt(config.getValueByAlias('maxNumber'), Infinity);
		this._overlaySize = config.getValueByAlias<UUIModalSidebarSize>('overlaySize') ?? 'small';
	}

	#parseInt(value: unknown, fallback: number): number {
		const num = Number(value);
		return num > 0 ? num : fallback;
	}

	@state()
	private _overlaySize?: UUIModalSidebarSize;

	@state()
	private _hideAnchor?: boolean;

	@state()
	private _min = 0;

	@state()
	private _max = Infinity;

	@state()
	private _alias?: string;

	@state()
	private _variantId?: string;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context.alias, (alias) => (this._alias = alias));
			this.observe(context.variantId, (variantId) => (this._variantId = variantId?.toString() || 'invariant'));
		});
	}

	#onChange(event: CustomEvent & { target: UmbMultiUrlPickerElement }) {
		this.value = event.target.urls;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-multi-url-picker
				.alias=${this._alias}
				.max=${this._max}
				.min=${this._min}
				.overlaySize=${this._overlaySize}
				.urls=${this.value ?? []}
				.variantId=${this._variantId}
				?hide-anchor=${this._hideAnchor}
				@change=${this.#onChange}>
			</umb-multi-url-picker>
		`;
	}
}

export default UmbPropertyEditorUIMultiUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multi-url-picker': UmbPropertyEditorUIMultiUrlPickerElement;
	}
}
