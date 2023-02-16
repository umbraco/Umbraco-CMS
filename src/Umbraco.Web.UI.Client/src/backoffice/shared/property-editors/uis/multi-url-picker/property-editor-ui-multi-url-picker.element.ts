import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbInputMultiUrlPickerElement } from '../../../components/input-multi-url-picker/input-multi-url-picker.element';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-multi-url-picker
 */
@customElement('umb-property-editor-ui-multi-url-picker')
export class UmbPropertyEditorUIMultiUrlPickerElement extends UmbLitElement {
	static styles = [UUITextStyles];

	private _value: string[] = [];
	@property({ type: Array })
	public get value(): string[] {
		return this._value;
	}
	public set value(value: string[]) {
		this._value = value || [];
	}
	/*
	@property({ type: Array, attribute: false })
	public set config(config: DataTypePropertyData[]) {
		const overlaySize = config.find((x) => x.alias === 'overlaySize');
		if (overlaySize) this._overlaySize = overlaySize.value as OverlaySize;

		const hideAnchor = config.find((x) => x.alias === 'hideAnchor');
		if (hideAnchor) this._hideAnchor = hideAnchor.value;
	}*/

	@state()
	private _hideAnchor?: boolean;

	private _onChange(event: CustomEvent) {
		//this._value = (event.target as UmbInputMultiUrlPickerElement);
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-multi-url-picker
			@change="${this._onChange}"
			?hide-anchor="${this._hideAnchor}"
			.selectedKeys="${this._value}"></umb-input-multi-url-picker>`;
	}
}

export default UmbPropertyEditorUIMultiUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multi-url-picker': UmbPropertyEditorUIMultiUrlPickerElement;
	}
}
