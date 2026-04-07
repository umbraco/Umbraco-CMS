import { UmbCollectionViewElementBase } from '../umb-collection-view-element-base.js';
import type { ManifestCollectionViewTableKind, MetaCollectionViewTableKindColumn } from './types.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbTableSelectedEvent,
	UmbTableElement,
	UmbTableDeselectedEvent,
	UmbTableItem,
	UmbTableColumn,
} from '@umbraco-cms/backoffice/components';
import type { UmbWithOptionalDescriptionModel } from '@umbraco-cms/backoffice/models';

import './entity-name-table-column-layout.element.js';

@customElement('umb-table-collection-view')
export class UmbTableCollectionViewElement extends UmbCollectionViewElementBase {
	override set manifest(value: ManifestCollectionViewTableKind | undefined) {
		super.manifest = value;
		this._manifestColumns = value?.meta?.columns ?? [];
	}
	override get manifest(): ManifestCollectionViewTableKind | undefined {
		return super.manifest as ManifestCollectionViewTableKind | undefined;
	}

	@state()
	private _tableColumns: Array<UmbTableColumn> = [];

	@state()
	private _manifestColumns: Array<MetaCollectionViewTableKindColumn> = [];

	@state()
	private _tableRows: Array<UmbTableItem> = [];

	#hasDescriptions = false;

	#buildTableColumns() {
		const nameColumn: UmbTableColumn = {
			name: this.localize.term('general_name'),
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

	override updated(changedProperties: any) {
		if (
			changedProperties.has('_items') ||
			changedProperties.has('_itemHrefs') ||
			changedProperties.has('_manifestColumns')
		) {
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

			const manifestColumnData = this._manifestColumns.map((col) => ({
				columnAlias: col.field,
				value: (item as unknown as Record<string, unknown>)[col.field],
			}));

			return {
				id: item.unique,
				icon: item.icon,
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
