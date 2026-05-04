import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

interface UmbEntityNameTableColumnValue {
	name: string;
	href?: string;
}

/**
 * Table column layout element that renders an entity name, optionally as a link.
 * @element umb-entity-name-table-column-layout
 */
@customElement('umb-entity-name-table-column-layout')
export class UmbEntityNameTableColumnLayoutElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbEntityNameTableColumnValue;

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
