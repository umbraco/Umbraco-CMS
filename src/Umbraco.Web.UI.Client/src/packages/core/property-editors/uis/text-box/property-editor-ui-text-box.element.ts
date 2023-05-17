import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-property-editor-ui-text-box')
export class UmbPropertyEditorUITextBoxElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@state()
	private _type = 'text';

	@state()
	private _maxChars?: number;

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyPresentationModel>) {
		const inputType = config.find((x) => x.alias === 'inputType');
		if (inputType) this._type = inputType.value;

		const maxChars = config.find((x) => x.alias === 'maxChars');
		if (maxChars) this._maxChars = maxChars.value;
	}

	private onInput(e: InputEvent) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<uui-input
			.value=${this.value}
			type="${this._type}"
			maxlength="${ifDefined(this._maxChars)}"
			@input=${this.onInput}></uui-input>`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUITextBoxElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-text-box': UmbPropertyEditorUITextBoxElement;
	}
}
