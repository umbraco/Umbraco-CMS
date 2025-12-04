import type { UmbEditableMediaCollectionItemModel } from '../../../types.js';
import { customElement, html, nothing, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-media-table-column-system-value')
export class UmbMediaTableColumnSystemValueElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbEditableMediaCollectionItemModel;

	#getPropertyValueByAlias() {
		const alias = this.column.alias;
		const item = this.value.item;
		switch (alias) {
			case 'contentTypeAlias':
				return item.contentTypeAlias;
			case 'createDate':
				return item.createDate.toLocaleString();
			case 'name':
				return item.name;
			case 'creator':
			case 'owner':
				return item.creator;
			case 'sortOrder':
				return item.sortOrder;
			case 'updateDate':
				return item.updateDate.toLocaleString();
			case 'updater':
				return item.updater;
			default:
				return item.values?.find((value) => value.alias === alias)?.value ?? '';
		}
	}

	override render() {
		if (!this.value) return nothing;
		const value = this.#getPropertyValueByAlias();
		return when(
			this.column.labelTemplate,
			() => html`<umb-ufm-render inline .markdown=${this.column.labelTemplate} .value=${{ value }}></umb-ufm-render>`,
			() => html`${value}`,
		);
	}
}

export default UmbMediaTableColumnSystemValueElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-table-column-system-value': UmbMediaTableColumnSystemValueElement;
	}
}
