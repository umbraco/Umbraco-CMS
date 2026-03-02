import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, LitElement, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-entity-name-table-column-layout')
export class UmbEntityNameTableColumnLayoutElement extends LitElement {
	@property({ attribute: false })
	value!: { name: string; href?: string };

	override render() {
		if (!this.value) return nothing;

		if (this.value.href) {
			return html`<a href=${this.value.href}>${this.value.name}</a>`;
		}

		return html`<span>${this.value.name}</span>`;
	}

	static override styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-name-table-column-layout': UmbEntityNameTableColumnLayoutElement;
	}
}
