import { UmbCollectionViewElementBase } from '../umb-collection-view-element-base.js';
import type { UmbCollectionItemModel } from '../../types.js';
import type { ManifestCollectionViewTableKind, MetaCollectionViewTableKindColumn } from './types.js';
import { css, customElement, html, nothing, state, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbTableSelectedEvent,
	UmbTableElement,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableColumn,
} from '@umbraco-cms/backoffice/components';
import type { UmbWithOptionalDescriptionModel } from '@umbraco-cms/backoffice/models';
import { UmbElementControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';

import './entity-name-table-column-layout.element.js';

@customElement('umb-table-collection-view')
export class UmbTableCollectionViewElement extends UmbCollectionViewElementBase<
	UmbCollectionItemModel,
	ManifestCollectionViewTableKind
> {
	@state()
	private _tableColumns: Array<UmbTableColumn> = [];

	@state()
	private _tableRows: Array<UmbTableItem> = [];

	get #manifestColumns(): Array<MetaCollectionViewTableKindColumn> {
		return this.manifest?.meta?.columns ?? [];
	}

	#hasDescriptions = false;

	#rowContexts = new Map<string, { host: UmbElementControllerHost; entityContext: UmbEntityContext }>();

	#onRowRendered = (element: HTMLElement, item: UmbTableItem) => {
		const existing = this.#rowContexts.get(item.id);
		if (existing) {
			existing.entityContext.setEntityType(item.entityType);
			existing.entityContext.setUnique(item.id);
			return;
		}

		const host = new UmbElementControllerHost(element);
		host.hostConnected();
		const entityContext = new UmbEntityContext(host);
		entityContext.setEntityType(item.entityType);
		entityContext.setUnique(item.id);

		this.#rowContexts.set(item.id, { host, entityContext });
	};

	#buildTableColumns() {
		const nameColumn: UmbTableColumn = {
			name: this.localize.term('general_name'),
			alias: 'name',
			elementName: 'umb-entity-name-table-column-layout',
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

		const descriptionColumn: UmbTableColumn = {
			name: this.localize.term('general_description'),
			alias: 'description',
		};

		this._tableColumns = [
			nameColumn,
			...(this.#hasDescriptions ? [descriptionColumn] : []),
			...manifestColumns,
			entityActionsColumn,
		];
	}

	override updated(changedProperties: PropertyValues) {
		if (changedProperties.has('_items') || changedProperties.has('_itemHrefs') || changedProperties.has('manifest')) {
			this.#createTableRows();
		}
	}

	#createTableRows() {
		this.#hasDescriptions = this._items.some(
			(item) => ((item as unknown as UmbWithOptionalDescriptionModel).description ?? '').length > 0,
		);
		this.#buildTableColumns();

		this._tableRows = this._items.map((item) => {
			const href = item.unique ? this._itemHrefs.get(item.unique) : undefined;

			const manifestColumnData = this.#manifestColumns.map((col) => ({
				columnAlias: col.field,
				value: (item as unknown as Record<string, unknown>)[col.field],
			}));

			return {
				id: item.unique,
				icon: item.icon,
				entityType: item.entityType,
				selectable: this._isSelectableItem(item),
				data: [
					{
						columnAlias: 'name',
						value: { name: item.name, href },
					},
					...(this.#hasDescriptions
						? [
								{
									columnAlias: 'description',
									value: (item as unknown as UmbWithOptionalDescriptionModel).description ?? '',
								},
							]
						: []),
					...manifestColumnData,
					{
						columnAlias: 'entityActions',
						value: { name: item.name },
					},
				],
			};
		});

		// Clean up row contexts for items that no longer exist
		const currentIds = new Set(this._tableRows.map((row) => row.id));
		for (const [id, ctx] of this.#rowContexts) {
			if (!currentIds.has(id)) {
				ctx.host.destroy();
				this.#rowContexts.delete(id);
			}
		}
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

	override disconnectedCallback() {
		super.disconnectedCallback();
		for (const [, ctx] of this.#rowContexts) {
			ctx.host.destroy();
		}
		this.#rowContexts.clear();
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
				.onRowRendered=${this.#onRowRendered}
				@selected=${this.#onSelected}
				@deselected="${this.#onDeselected}"></umb-table>
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

export { UmbTableCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-table-collection-view': UmbTableCollectionViewElement;
	}
}
