import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-language-root-table-delete-column-layout')
export class UmbLanguageRootTableDeleteColumnLayoutElement extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	value!: any;

	render() {
		if (!this.value.show) return nothing;

		return html`<uui-button color="danger" look="primary" compact label="delete"></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-table-delete-column-layout': UmbLanguageRootTableDeleteColumnLayoutElement;
	}
}
