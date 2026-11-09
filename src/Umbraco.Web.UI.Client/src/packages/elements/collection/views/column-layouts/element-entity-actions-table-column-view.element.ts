import { UmbElementTreeItemAncestorsEntityContext } from '../../../tree/element-tree-item-ancestors.entity-context.js';
import type { UmbElementTreeItemModel } from '../../../tree/types.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-element-entity-actions-table-column-view')
export class UmbElementEntityActionsTableColumnViewElement extends UmbLitElement {
	#ancestorsContext = new UmbElementTreeItemAncestorsEntityContext(this);

	@property({ attribute: false })
	public set value(value: UmbElementTreeItemModel | undefined) {
		this.#value = value;
		this.#ancestorsContext.setParentUnique(value?.parent?.unique);
	}
	public get value(): UmbElementTreeItemModel | undefined {
		return this.#value;
	}
	#value?: UmbElementTreeItemModel;

	override render() {
		if (!this.#value) return nothing;

		return html`
			<umb-entity-actions-table-column-view
				.value=${{ unique: this.#value.unique, entityType: this.#value.entityType }}>
			</umb-entity-actions-table-column-view>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-entity-actions-table-column-view': UmbElementEntityActionsTableColumnViewElement;
	}
}
