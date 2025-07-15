import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-date-table-column-view')
export class UmbDateTableColumnViewElement extends UmbLitElement {
	@property({ attribute: false })
	value?: string;

	override render() {
		if (!this.value) return nothing;
		const date = new Date(this.value);
		return html`${date.toLocaleString()}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-table-column-view': UmbDateTableColumnViewElement;
	}
}
