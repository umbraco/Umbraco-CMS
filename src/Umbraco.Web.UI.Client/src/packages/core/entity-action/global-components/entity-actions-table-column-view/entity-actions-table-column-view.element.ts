import type { UmbEntityModel, UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';
import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-entity-actions-table-column-view')
export class UmbEntityActionsTableColumnViewElement extends UmbLitElement {
	@property({ attribute: false })
	value?: UmbEntityModel | UmbNamedEntityModel;

	override render() {
		if (!this.value) return nothing;

		return html`
			<umb-entity-actions-bundle
				.entityType=${this.value.entityType}
				.unique=${this.value.unique}
				.label=${this.localize.term('actions_viewActionsFor', [(this.value as any).name])}>
			</umb-entity-actions-bundle>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-table-column-view': UmbEntityActionsTableColumnViewElement;
	}
}
