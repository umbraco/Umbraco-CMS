import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/data-type';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-property-editor-ui-text-box')
export class UmbPropertyEditorUITextBoxElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@state()
	private _type = 'text';

	@state()
	private _maxChars?: number;

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		this._type = config.getValueByAlias('inputType') ?? 'text';
		this._maxChars = config.getValueByAlias('maxChars');
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
