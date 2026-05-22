import type { UmbContentTreeItemModel } from '../../types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UmbSortChildrenOfModalElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-sort-children-of-content-modal')
export class UmbSortChildrenOfContentModalElement extends UmbSortChildrenOfModalElement<UmbContentTreeItemModel> {
	#localizeDateOptions: Intl.DateTimeFormatOptions = {
		day: 'numeric',
		month: 'short',
		year: 'numeric',
		hour: 'numeric',
		minute: '2-digit',
	};

	#appCulture?: string;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
			this.observe(
				context?.appLanguageCulture,
				(appCulture) => {
					this.#appCulture = appCulture;
					if (this._tableItems.length > 0) {
						// Patch names in-place so any user drag-sort or column-order is preserved.
						this.#updateTableItemNames();
					}
				},
				'umbObserveAppLanguageCulture',
			);
		});
	}

	protected override _setTableColumns() {
		this._tableColumns = [
			{
				name: this.localize.term('general_name'),
				alias: 'name',
				allowSorting: this._hasMorePages() === false,
			},
			{
				name: this.localize.term('content_createDate'),
				alias: 'createDate',
				allowSorting: this._hasMorePages() === false,
			},
		];
	}

	protected override _createTableItems() {
		this._tableItems = this._children.map((treeItem) => {
			// TODO: implement ItemDataResolver for document and media. This will also fix the hard-coded icon.
			return {
				id: treeItem.unique,
				icon: 'icon-document',
				data: [
					{
						columnAlias: 'name',
						value: this.#resolveName(treeItem),
					},
					{
						columnAlias: 'createDate',
						value: this.localize.date(treeItem.createDate, this.#localizeDateOptions),
					},
				],
			};
		});
	}

	#updateTableItemNames() {
		this._tableItems = this._tableItems.map((tableItem) => {
			const treeItem = this._children.find((child) => child.unique === tableItem.id);
			if (!treeItem) return tableItem;
			return {
				...tableItem,
				data: tableItem.data.map((column) =>
					column.columnAlias === 'name' ? { ...column, value: this.#resolveName(treeItem) } : column,
				),
			};
		});
	}

	#resolveName(treeItem: UmbContentTreeItemModel): string {
		// The shared UmbContentTreeItemModel type used by this modal does not declare variants, so the cast stays local here.
		const variants = (
			treeItem as UmbContentTreeItemModel & { variants?: Array<{ name?: string; culture: string | null }> }
		).variants;
		if (!variants?.length) {
			return treeItem.name;
		}

		// Invariant content: the only variant has culture === null — use treeItem.name.
		if (variants[0].culture === null) {
			return treeItem.name;
		}

		const currentVariant = this.#appCulture
			? variants.find((variant) => variant.culture === this.#appCulture)
			: undefined;
		if (currentVariant?.name) {
			return currentVariant.name;
		}

		// No name in the current language — fall back to any named variant, parenthesised so it's visibly a fallback.
		const fallbackName = variants.find((variant) => variant.name)?.name ?? treeItem.name;
		return fallbackName ? `(${fallbackName})` : treeItem.name;
	}

	protected override _sortCompare(columnAlias: string, valueA: unknown, valueB: unknown): number {
		if (columnAlias === 'createDate') {
			return Date.parse(valueA as string) - Date.parse(valueB as string);
		}

		return super._sortCompare(columnAlias, valueA, valueB);
	}

	protected override _onLoadMore(event: PointerEvent): void {
		super._onLoadMore(event);
		// TODO: make nicer API for enable/disable orderBy for individual columns
		const allowOrderBy = this._hasMorePages() === false;
		this._tableColumns[0].allowSorting = allowOrderBy;
		this._tableColumns[1].allowSorting = allowOrderBy;
	}
}

export { UmbSortChildrenOfContentModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-sort-children-of-content-modal']: UmbSortChildrenOfContentModalElement;
	}
}
