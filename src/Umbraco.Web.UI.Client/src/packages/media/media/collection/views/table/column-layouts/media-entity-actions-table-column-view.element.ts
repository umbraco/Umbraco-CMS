import type { UmbMediaCollectionItemModel } from '../../../types.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-media-entity-actions-table-column-view')
export class UmbMediaEntityActionsTableColumnViewElement extends UmbLitElement {
	@property({ attribute: false })
	value?: UmbMediaCollectionItemModel;

	override render() {
		if (!this.value) return nothing;

		return html`
			<umb-entity-actions-table-column-view .value=${{ unique: this.value.unique, entityType: this.value.entityType }}>
			</umb-entity-actions-table-column-view>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-entity-actions-table-column-view': UmbMediaEntityActionsTableColumnViewElement;
	}
}
