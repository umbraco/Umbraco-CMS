import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-entity-actions-table-column-view')
export class UmbEntityActionsTableColumnViewElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	@property({ attribute: false })
	column!: UmbTableColumn;

	@property({ attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value: { name?: string } = {};

	override render() {
		return html`
			<umb-entity-actions-bundle
				.label=${this.localize.string(this.value?.name)}>
			</umb-entity-actions-bundle>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-table-column-view': UmbEntityActionsTableColumnViewElement;
	}
}
