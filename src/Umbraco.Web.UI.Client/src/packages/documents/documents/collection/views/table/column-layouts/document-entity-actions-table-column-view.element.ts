import type { UmbDocumentCollectionItemModel } from '../../../types.js';
import { UmbAncestorsEntityContext } from '@umbraco-cms/backoffice/entity';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-entity-actions-table-column-view')
export class UmbDocumentEntityActionsTableColumnViewElement extends UmbLitElement {
	#ancestorContext = new UmbAncestorsEntityContext(this);

	@property({ attribute: false })
	public set value(value: UmbDocumentCollectionItemModel) {
		this.#value = value;
		this.#ancestorContext.setAncestors(this.#value?.ancestors ?? []);
	}
	public get value(): UmbDocumentCollectionItemModel | undefined {
		return this.#value;
	}
	#value?: UmbDocumentCollectionItemModel | undefined;

	override render() {
		if (!this.#value) return nothing;

		// TODO: Missing name to parse on
		return html`
			<umb-entity-actions-table-column-view
				.value=${{ unique: this.#value.unique, entityType: this.#value.entityType }}>
			</umb-entity-actions-table-column-view>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-entity-actions-table-column-view': UmbDocumentEntityActionsTableColumnViewElement;
	}
}
