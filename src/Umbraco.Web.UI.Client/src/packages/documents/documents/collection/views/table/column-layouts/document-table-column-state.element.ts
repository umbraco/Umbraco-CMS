import { UmbDocumentItemDataResolver } from '../../../../item/index.js';
import type { UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import type { UmbDocumentVariantState } from '../../../../variant-state.js';
import { getDocumentVariantStateTagConfig } from '../../../../variant-state/utils.js';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-document-table-column-state')
export class UmbDocumentTableColumnStateElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	#resolver = new UmbDocumentItemDataResolver(this);

	@state()
	private _state?: UmbDocumentVariantState | null;

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbEditableDocumentCollectionItemModel) {
		this.#value = value;

		if (value.item) {
			this.#resolver.setData(value.item);
		}
	}
	public get value(): UmbEditableDocumentCollectionItemModel {
		return this.#value;
	}
	#value!: UmbEditableDocumentCollectionItemModel;

	constructor() {
		super();
		this.#resolver.observe(this.#resolver.state, (state) => (this._state = state));
	}

	override render() {
		if (this._state === undefined) return nothing;
		const { color, label } = getDocumentVariantStateTagConfig(this._state, this.localize);
		return html`<uui-tag color=${color} look="secondary">${label}</uui-tag>`;
	}
}

export default UmbDocumentTableColumnStateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-state': UmbDocumentTableColumnStateElement;
	}
}
