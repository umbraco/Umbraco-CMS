import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-property-editor-number')
export class UmbPropertyEditorNumberElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-input {
				width: 100%;
			}
		`,
	];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	private onInput(e: InputEvent) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-editor-change', { bubbles: true, composed: true }));
	}

	render() {
		return html`<uui-input .value=${this.value} type="number" @input=${this.onInput}></uui-input>`;
	}
}

export default UmbPropertyEditorNumberElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-number': UmbPropertyEditorNumberElement;
	}
}
