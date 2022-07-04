import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-property-editor-textarea')
class UmbPropertyEditorTextarea extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-textarea {
				width: 100%;
			}
		`,
	];

	@property()
	value = '';

	private onInput(e: InputEvent) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-editor-change', { bubbles: true, composed: true }));
	}

	render() {
		return html`<uui-textarea .value=${this.value} @input=${this.onInput}></uui-textarea>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-textarea': UmbPropertyEditorTextarea;
	}
}
