import type { UmbContentTreeItemModel } from '../../types.js';
import { customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
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

	protected override _setTableColumns() {
		this._tableColumns = [
			{
				name: this.localize.term('general_name'),
				alias: 'name',
				allowSorting: true,
			},
			{
				name: this.localize.term('content_createDate'),
				alias: 'createDate',
				allowSorting: true,
			},
		];
	}

	protected override _createTableItems() {
		this._tableItems = this._children.map((treeItem) => {
			// TODO: implement ItemDataResolver for document and media
			// This will fix both the icon and the variant name
			return {
				id: treeItem.unique,
				icon: 'icon-document',
				data: [
					{
						columnAlias: 'name',
						value: treeItem.name,
					},
					{
						columnAlias: 'createDate',
						value: this.localize.date(treeItem.createDate, this.#localizeDateOptions),
					},
				],
			};
		});
	}

	protected override _sortCompare(columnAlias: string, valueA: unknown, valueB: unknown): number {
		if (columnAlias === 'createDate') {
			return Date.parse(valueA as string) - Date.parse(valueB as string);
		}

		return super._sortCompare(columnAlias, valueA, valueB);
	}
}

export { UmbSortChildrenOfContentModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-sort-children-of-content-modal']: UmbSortChildrenOfContentModalElement;
	}
}
