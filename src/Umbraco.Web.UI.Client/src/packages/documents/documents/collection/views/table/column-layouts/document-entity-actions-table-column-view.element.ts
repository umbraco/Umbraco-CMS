import type { UmbDocumentCollectionItemModel } from '../../../types.js';
import { UmbAncestorsEntityContext } from '@umbraco-cms/backoffice/entity';
import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-entity-actions-table-column-view')
export class UmbDocumentEntityActionsTableColumnViewElement extends UmbLitElement {
	@property({ attribute: false })
	public get value(): UmbDocumentCollectionItemModel | undefined {
		return this._value;
	}
	public set value(value: UmbDocumentCollectionItemModel | undefined) {
		this._value = value;
		this.#ancestorContext.setAncestors(this._value?.ancestors ?? []);
	}

	private _value?: UmbDocumentCollectionItemModel | undefined;

	#ancestorContext = new UmbAncestorsEntityContext(this);

	override render() {
		if (!this._value) return nothing;

		return html`
			<umb-entity-actions-table-column-view
				.value=${{ unique: this._value.unique, entityType: this._value.entityType }}>
			</umb-entity-actions-table-column-view>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		['umb-document-entity-actions-table-column-view']: UmbDocumentEntityActionsTableColumnViewElement;
	}
}
