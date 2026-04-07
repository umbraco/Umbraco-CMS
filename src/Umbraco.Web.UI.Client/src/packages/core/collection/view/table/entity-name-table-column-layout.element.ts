import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-name-table-column-layout')
export class UmbEntityNameTableColumnLayoutElement extends UmbLitElement {
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
