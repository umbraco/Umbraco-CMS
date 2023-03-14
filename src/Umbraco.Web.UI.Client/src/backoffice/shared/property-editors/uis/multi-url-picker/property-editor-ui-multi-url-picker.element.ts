import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { UmbInputMultiUrlPickerElement } from '../../../../shared/components/input-multi-url-picker/input-multi-url-picker.element';
import { UmbLinkPickerLink } from '../../../../shared/modals/link-picker';
import { UmbPropertyEditorElement } from '@umbraco-cms/property-editor';
import { UmbLitElement } from '@umbraco-cms/element';
import { DataTypePropertyModel } from '@umbraco-cms/backend-api';

/**
 * @element umb-property-editor-ui-multi-url-picker
 */

@customElement('umb-property-editor-ui-multi-url-picker')
export class UmbPropertyEditorUIMultiUrlPickerElement extends UmbLitElement implements UmbPropertyEditorElement {
	static styles = [UUITextStyles];

	@property({ type: Array })
	value: UmbLinkPickerLink[] = [];

	@property({ type: Array, attribute: false })
	public set config(config: DataTypePropertyModel[]) {
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

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputMultiUrlPickerElement).urls;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-multi-url-picker
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
