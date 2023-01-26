import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-language-root-table-name-column-layout')
export class UmbLanguageRootTableNameColumnLayoutElement extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	value!: { key: string; name: string };

	render() {
		if (!this.value) return nothing;

		return html`<a href=${'section/settings/language/edit/' + this.value.key}>${this.value.name}</a>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-table-name-column-layout': UmbLanguageRootTableNameColumnLayoutElement;
	}
}
