import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import type { UUIModalSidebarSize } from '@umbraco-ui/uui';
import { UmbInputMultiUrlPickerElement } from '../../../../shared/components/input-multi-url-picker/input-multi-url-picker.element';
import { UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN } from '../../../../shared/components/workspace-property/workspace-property.context';
import { UmbLinkPickerLink } from '@umbraco-cms/backoffice/modal';
import { UmbPropertyEditorElement } from '@umbraco-cms/backoffice/property-editor';
import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-multi-url-picker
 */
@customElement('umb-property-editor-ui-multi-url-picker')
export class UmbPropertyEditorUIMultiUrlPickerElement extends UmbLitElement implements UmbPropertyEditorElement {
	static styles = [UUITextStyles];

	@property({ type: Array })
	value: UmbLinkPickerLink[] = [];

	@property({ type: Array, attribute: false })
	public set config(config: DataTypePropertyPresentationModel[]) {
		const overlaySize = config.find((x) => x.alias === 'overlaySize');
		if (overlaySize) this._overlaySize = overlaySize.value;

		const hideAnchor = config.find((x) => x.alias === 'hideAnchor');
		if (hideAnchor) this._hideAnchor = hideAnchor.value;

		const ignoreUserStartNodes = config.find((x) => x.alias === 'ignoreUserStartNodes');
		if (ignoreUserStartNodes) this._ignoreUserStartNodes = ignoreUserStartNodes.value;

		const maxNumber = config.find((x) => x.alias === 'maxNumber');
		if (maxNumber) this._maxNumber = maxNumber.value;

		const minNumber = config.find((x) => x.alias === 'minNumber');
		if (minNumber) this._minNumber = minNumber.value;
	}
	@state()
	private _overlaySize?: UUIModalSidebarSize;

	@state()
	private _hideAnchor?: boolean;

	@state()
	private _ignoreUserStartNodes?: boolean;

	@state()
	private _maxNumber?: number;

	@state()
	private _minNumber?: number;

	@state()
	private _alias?: string;

	@state()
	private _propertyVariantId?: string;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN, (context) => {
			this.observe(context.alias, (alias) => {
				this._alias = alias;
			});
			this.observe(context.variantId, (variantId) => {
				this._propertyVariantId = variantId?.toString() || 'invariant';
			});
		});
	}

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputMultiUrlPickerElement).urls;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-multi-url-picker
			.alias="${this._alias}"
			.variantId="${this._propertyVariantId}"
			@change="${this._onChange}"
			.overlaySize="${this._overlaySize}"
			?hide-anchor="${this._hideAnchor}"
			.ignoreUserStartNodes=${this._ignoreUserStartNodes}
			.max=${this._maxNumber}
			.min=${this._minNumber}
			.urls="${this.value}"></umb-input-multi-url-picker>`;
	}
}

export default UmbPropertyEditorUIMultiUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multi-url-picker': UmbPropertyEditorUIMultiUrlPickerElement;
	}
}
