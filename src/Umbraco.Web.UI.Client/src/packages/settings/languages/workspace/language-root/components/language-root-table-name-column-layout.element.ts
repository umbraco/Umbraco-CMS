import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, LitElement, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-language-root-table-name-column-layout')
export class UmbLanguageRootTableNameColumnLayoutElement extends LitElement {
	@property({ attribute: false })
	value!: { isoCode: string; name: string };

	render() {
		if (!this.value) return nothing;

		return html`<a href=${'section/settings/workspace/language/edit/' + this.value.isoCode}>${this.value.name}</a>`;
	}

	static styles = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-table-name-column-layout': UmbLanguageRootTableNameColumnLayoutElement;
	}
}
