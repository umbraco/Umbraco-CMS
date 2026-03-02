import { UmbCollectionViewElementBase } from '../umb-collection-view-element-base.js';
import type { UmbCollectionItemModel } from '../../types.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbTableSelectedEvent,
	UmbTableElement,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableColumn,
} from '@umbraco-cms/backoffice/components';

import './entity-name-table-column-layout.element.js';

@customElement('umb-table-collection-view')
export class UmbTableCollectionViewElement extends UmbCollectionViewElementBase {
	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'name',
			elementName: 'umb-entity-name-table-column-layout',
		},
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableRows: Array<UmbTableItem> = [];

	override updated(changedProperties: any) {
		if (changedProperties.has('_items')) {
			this.#createTableRows();
		}
	}

	#createTableRows() {
		this._tableRows = this._items.map((item) => {
			const href = item.unique ? this._itemHrefs.get(item.unique) : undefined;

			return {
				id: item.unique,
				icon: item.icon,
				selectable: this._isSelectableItem(item),
				data: [
					{
						columnAlias: 'name',
						value: { name: item.name, href },
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: item.entityType,
								unique: item.unique,
								name: item.name,
							}}></umb-entity-actions-table-column-view>`,
					},
				],
			};
		});
	}

	#onSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const itemId = event.getItemId();

		// We get the same event for both single and multiple selection.
		if (itemId) {
			this._selectItem(itemId);
		} else {
			const target = event.target as UmbTableElement;
			this._setSelection(target.selection);
		}
	}

	#onDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const itemId = event.getItemId();

		// We get the same event for both single and multiple deselection.
		if (itemId) {
			this._deselectItem(itemId);
		} else {
			const target = event.target as UmbTableElement;
			this._setSelection(target.selection);
		}
	}

	override render() {
		if (this._loading) return nothing;
		return html`
			<umb-table
				.config=${{
					allowSelection: this._selectable,
					allowSelectAll: this._multiple,
					selectOnly: this._selectOnly,
				}}
				.columns=${this._tableColumns}
				.items=${this._tableRows}
				.selection=${this._selection}
				@selected="${this.#onSelected}"
				@deselected="${this.#onDeselected}"></umb-table>
		`;
	}

	#renderRow(item: UmbCollectionItemModel) {
		const href = item.unique ? this._itemHrefs.get(item.unique) : undefined;
		return html` <umb-entity-collection-item-card .item=${item} href=${href ?? nothing}>
		</umb-entity-collection-item-card>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export { UmbTableCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-table-collection-view': UmbTableCollectionViewElement;
	}
}
