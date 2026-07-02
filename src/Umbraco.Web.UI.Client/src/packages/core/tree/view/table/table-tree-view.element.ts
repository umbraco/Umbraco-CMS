import type { UmbTreeItemModel } from '../../types.js';
import { UmbTreeViewElementBase } from '../tree-view-element-base.js';
import type { ManifestTreeViewTableKind, MetaTreeViewTableKindColumn } from './types.js';
import { UmbTableTreeViewRowController } from './table-tree-view-row.controller.js';
import {
	css,
	customElement,
	html,
	nothing,
	property,
	state,
	type PropertyValues,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { getItemFallbackIcon } from '@umbraco-cms/backoffice/entity-item';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableSelectedEvent,
} from '@umbraco-cms/backoffice/components';

import './tree-name-table-column-layout.element.js';
import '@umbraco-cms/backoffice/entity-action';

@customElement('umb-table-tree-view')
export class UmbTableTreeViewElement extends UmbTreeViewElementBase<UmbTreeItemModel> {
	private _items: Array<UmbTreeItemModel> = [];

	@state()
	private _hideTreeItemActions = false;

	@state()
	private _tableRows: Array<UmbTableItem> = [];

	#manifest?: ManifestTreeViewTableKind;

	@property({ attribute: false })
	set manifest(value: ManifestTreeViewTableKind | undefined) {
		this.#manifest = value;
		this.#createTableRows();
	}
	get manifest(): ManifestTreeViewTableKind | undefined {
		return this.#manifest;
	}

	#tableConfig: UmbTableConfig = { allowSelection: false, allowSelectAll: false, selectOnly: false };

	get #manifestColumns(): Array<MetaTreeViewTableKindColumn> {
		return this.#manifest?.meta?.columns ?? [];
	}

	#itemMap = new Map<string, UmbTreeItemModel>();
	#rows = new Map<string, UmbTableTreeViewRowController>();

	#onRowRendered = (element: HTMLElement | undefined, item: UmbTableItem) => {
		if (!element) {
			this.#rows.get(item.id)?.destroy();
			this.#rows.delete(item.id);
			return;
		}

		const existing = this.#rows.get(item.id);
		if (existing) {
			existing.setItem(item.entityType, item.id, this.#itemMap.get(item.id));
			return;
		}

		const row = new UmbTableTreeViewRowController(element, item.entityType, item.id, this.#itemMap.get(item.id));

		// Register before observers so synchronous emissions see the row.
		this.#rows.set(item.id, row);

		row.observeApi({
			onNoAccessChange: () => this.#updateRowSelectable(item.id),
			onPathChange: () => {
				const updated = this.#updateRowHref(item.id);
				if (updated) this._tableRows = updated;
			},
			onActiveChange: () => this.#updateRowActive(item.id),
		});
	};

	#updateRowSelectable(id: string) {
		const idx = this._tableRows.findIndex((r) => r.id === id);
		if (idx === -1) return;

		const row = this.#rows.get(id);
		const treeItem = this.#itemMap.get(id);
		const selectable = !(row?.currentNoAccess ?? false) && (treeItem ? this._isSelectableItem(treeItem) : false);

		if (this._tableRows[idx].selectable === selectable) return;

		this._tableRows = [
			...this._tableRows.slice(0, idx),
			{ ...this._tableRows[idx], selectable },
			...this._tableRows.slice(idx + 1),
		];
	}

	#updateRowHref(id: string, rows = this._tableRows): UmbTableItem[] | null {
		const idx = rows.findIndex((r) => r.id === id);
		if (idx === -1) return null;

		const row = this.#rows.get(id);
		const treeItem = this.#itemMap.get(id);
		if (!treeItem) return null;

		const href = this._selectable ? undefined : row?.currentPath || undefined;
		const nameData = rows[idx].data.find((d) => d.columnAlias === 'name');
		if (nameData?.value?.href === href) return null;

		const childrenIndicator = rows[idx].childrenIndicator;

		return [
			...rows.slice(0, idx),
			{
				...rows[idx],
				childrenIndicator: childrenIndicator ? { ...childrenIndicator, href } : undefined,
				data: rows[idx].data.map((d) => (d.columnAlias === 'name' ? { ...d, value: { ...d.value, href } } : d)),
			},
			...rows.slice(idx + 1),
		];
	}

	#updateRowActive(id: string) {
		const idx = this._tableRows.findIndex((r) => r.id === id);
		if (idx === -1) return;

		const isActive = this.#rows.get(id)?.currentIsActive ?? false;
		if (this._tableRows[idx].active === isActive) return;

		this._tableRows = [
			...this._tableRows.slice(0, idx),
			{ ...this._tableRows[idx], active: isActive },
			...this._tableRows.slice(idx + 1),
		];
	}

	protected override _gotTreeContext() {
		super._gotTreeContext();

		this.observe(
			this._treeContext?.currentPageItems,
			(items) => {
				this._items = items ?? [];
				this.#createTableRows();
			},
			'_observeCurrentPageItems',
		);

		this.observe(
			this._treeContext?.hideTreeItemActions,
			(value) => (this._hideTreeItemActions = value ?? false),
			'_observeHideTreeItemActions',
		);
	}

	#buildTableColumns(): Array<UmbTableColumn> {
		const nameColumn: UmbTableColumn = {
			name: this.localize.term('general_name'),
			alias: 'name',
			elementName: 'umb-tree-name-table-column-layout',
		};

		const manifestColumns: Array<UmbTableColumn> = this.#manifestColumns.map((col) => ({
			name: this.localize.string(col.label),
			alias: col.field,
		}));

		const entityActionsColumn: UmbTableColumn = {
			name: '',
			alias: 'entityActions',
			align: 'right',
			elementName: 'umb-entity-actions-table-column-view',
		};

		return [nameColumn, ...manifestColumns, ...(this._hideTreeItemActions ? [] : [entityActionsColumn])];
	}

	#toTableRow(item: UmbTreeItemModel): UmbTableItem {
		const id = item.unique;
		const icon = item.isFolder ? 'icon-folder' : (item.icon ?? getItemFallbackIcon());
		const row = this.#rows.get(id);
		const noAccess = row?.currentNoAccess ?? false;
		const href = this._selectable ? undefined : row?.currentPath || undefined;
		const isActive = row?.currentIsActive ?? false;
		const name = item.name;

		const manifestColumnData = this.#manifestColumns.map((col) => {
			const rawValue = (item as unknown as Record<string, unknown>)[col.field];
			if (col.valueType) {
				return {
					columnAlias: col.field,
					value: html`<umb-value-summary-extension
						.valueType=${col.valueType}
						.value=${rawValue}></umb-value-summary-extension>`,
				};
			}
			return { columnAlias: col.field, value: rawValue };
		});

		const onOpen = item.hasChildren ? () => this._treeContext?.open?.(item as UmbTreeItemModel) : undefined;

		return {
			id,
			icon,
			entityType: item.entityType,
			childrenIndicator: item.hasChildren ? { href, onOpen } : undefined,
			selectable: !noAccess && this._isSelectableItem(item as UmbTreeItemModel),
			active: isActive,
			data: [
				{
					columnAlias: 'name',
					value: {
						name,
						href,
						onOpen,
					},
				},
				...manifestColumnData,
				...(this._hideTreeItemActions ? [] : [{ columnAlias: 'entityActions', value: { name } }]),
			],
		};
	}

	#createTableRows() {
		const items = this._items;

		this.#itemMap.clear();
		for (const item of items) {
			this.#itemMap.set(item.unique, item);
		}

		this._tableRows = items.map((item) => this.#toTableRow(item));

		const currentIds = new Set(this._tableRows.map((row) => row.id));
		for (const [id, row] of this.#rows) {
			if (!currentIds.has(id)) {
				row.destroy();
				this.#rows.delete(id);
			}
		}
	}

	override willUpdate(changedProperties: PropertyValues) {
		super.willUpdate(changedProperties);
		if (changedProperties.has('_selectable') || changedProperties.has('_selectOnly')) {
			this.#tableConfig = {
				allowSelection: this._selectable,
				allowSelectAll: false,
				selectOnly: this._selectOnly,
			};
		}

		if (changedProperties.has('_selectable')) {
			let rows = this._tableRows;
			for (const id of this.#itemMap.keys()) {
				rows = this.#updateRowHref(id, rows) ?? rows;
			}
			if (rows !== this._tableRows) this._tableRows = rows;
		}

		if (changedProperties.has('_hideTreeItemActions')) {
			this.#createTableRows();
		}
	}

	#onSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const itemId = event.getItemId();
		if (itemId) this._selectItem(itemId);
	}

	#onDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const itemId = event.getItemId();
		if (itemId) this._deselectItem(itemId);
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		for (const row of this.#rows.values()) {
			row.destroy();
		}
		this.#rows.clear();
		this.#itemMap.clear();
	}

	override render() {
		if (!this._tableRows.length) return nothing;

		return html`
			<umb-table
				.config=${this.#tableConfig}
				.columns=${this.#buildTableColumns()}
				.items=${this._tableRows}
				.selection=${this._selection}
				.onRowRendered=${this.#onRowRendered}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}></umb-table>
			<umb-tree-pagination></umb-tree-pagination>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];
}

export default UmbTableTreeViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-table-tree-view': UmbTableTreeViewElement;
	}
}
