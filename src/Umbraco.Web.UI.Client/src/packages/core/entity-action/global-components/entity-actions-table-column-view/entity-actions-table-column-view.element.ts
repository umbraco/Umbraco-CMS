import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-entity-actions-table-column-view';
@customElement(elementName)
export class UmbEntityActionsTableColumnViewElement extends UmbLitElement {
	@property({ attribute: false })
	value?: UmbEntityModel;

	@state()
	_isOpen = false;

	override render() {
		if (!this.value) return nothing;

		return html`
			<umb-entity-actions-bundle .entityType=${this.value.entityType} .unique=${this.value.unique}>
			</umb-entity-actions-bundle>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbEntityActionsTableColumnViewElement;
	}
}
