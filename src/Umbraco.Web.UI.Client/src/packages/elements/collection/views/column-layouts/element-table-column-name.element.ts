import { UmbElementItemDataResolver } from '../../../item/data-resolver/element-item-data-resolver.js';
import type { UmbElementItemModel } from '../../../types.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

type UmbEditableElementItemModel = { item: UmbElementItemModel; editPath: string };

@customElement('umb-element-table-column-name')
export class UmbElementTableColumnNameElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	#resolver = new UmbElementItemDataResolver(this);

	@state()
	private _name = '';

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbEditableElementItemModel) {
		this.#value = value;

		if (value.item) {
			this.#resolver.setData(value.item);
		}
	}
	public get value(): UmbEditableElementItemModel {
		return this.#value;
	}
	#value!: UmbEditableElementItemModel;

	constructor() {
		super();
		this.#resolver.observe(this.#resolver.name, (name) => (this._name = name || ''));
	}

	override render() {
		if (!this.value) return nothing;
		if (!this.value.editPath) return nothing;
		if (!this._name) return nothing;
		return html`<uui-button compact href=${this.value.editPath} label=${this._name}></uui-button>`;
	}

	static override styles = [
		css`
			uui-button {
				text-align: left;
			}
		`,
	];
}

export default UmbElementTableColumnNameElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-table-column-name': UmbElementTableColumnNameElement;
	}
}
