import { UmbCollectionViewElementBase } from '../umb-collection-view-element-base.js';
import type { ManifestCollectionViewTableKind, MetaCollectionViewTableKindColumn } from './types.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbTableSelectedEvent,
	UmbTableElement,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableColumn,
} from '@umbraco-cms/backoffice/components';
import { UmbValueMinimalDisplayCoordinatorContext } from '@umbraco-cms/backoffice/value-minimal-display';

import './entity-name-table-column-layout.element.js';

@customElement('umb-table-collection-view')
export class UmbTableCollectionViewElement extends UmbCollectionViewElementBase {
	#manifest: ManifestCollectionViewTableKind | undefined;
	#coordinator = new UmbValueMinimalDisplayCoordinatorContext(this);

	@property({ attribute: false })
	public set manifest(value: ManifestCollectionViewTableKind | undefined) {
		this.#manifest = value;
		this._manifestColumns = value?.meta?.columns ?? [];
		this.#buildTableColumns();
	}
	public get manifest() {
		return this.#manifest;
	}

	@state()
	private _tableColumns: Array<UmbTableColumn> = [];

	@state()
	private _manifestColumns: Array<MetaCollectionViewTableKindColumn> = [];

	@state()
	private _tableRows: Array<UmbTableItem> = [];

	#buildTableColumns() {
		const nameColumn: UmbTableColumn = {
			name: 'Name',
			alias: 'name',
			elementName: 'umb-entity-name-table-column-layout',
		};

		const manifestColumns: Array<UmbTableColumn> = this._manifestColumns.map((col) => ({
			name: col.label,
			alias: col.field,
		}));

		const entityActionsColumn: UmbTableColumn = {
			name: '',
			alias: 'entityActions',
			align: 'right',
		};

		this._tableColumns = [nameColumn, ...manifestColumns, entityActionsColumn];
	}

	override updated(changedProperties: any) {
		if (
			changedProperties.has('_items') ||
			changedProperties.has('_itemHrefs') ||
			changedProperties.has('_manifestColumns')
		) {
			this.#preRegisterColumnValues();
			this.#createTableRows();
		}
	}

	#preRegisterColumnValues() {
		for (const col of this._manifestColumns) {
			if (!col.valueMinimalDisplayAlias) continue;
			const alias = col.valueMinimalDisplayAlias;
			const values = this._items.map((item) => (item as unknown as Record<string, unknown>)[col.field]);
			this.#coordinator.preRegister(alias, values);
		}
	}

	#createTableRows() {
		this._tableRows = this._items.map((item) => {
			const href = item.unique ? this._itemHrefs.get(item.unique) : undefined;

			const manifestColumnData = this._manifestColumns.map((col) => {
				const rawValue = (item as unknown as Record<string, unknown>)[col.field];
				if (col.valueMinimalDisplayAlias) {
					return {
						columnAlias: col.field,
						value: html`<umb-value-minimal-display
							.alias=${col.valueMinimalDisplayAlias}
							.value=${rawValue}></umb-value-minimal-display>`,
					};
				}
				return {
					columnAlias: col.field,
					value: rawValue,
				};
			});

			return {
				id: item.unique,
				icon: item.icon,
				selectable: this._isSelectableItem(item),
				data: [
					{
						columnAlias: 'name',
						value: { name: item.name, href },
					},
					...manifestColumnData,
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
