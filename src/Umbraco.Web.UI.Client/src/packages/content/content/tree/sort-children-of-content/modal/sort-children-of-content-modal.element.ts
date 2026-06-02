import type { UmbContentTreeItemModel } from '../../types.js';
import type { UmbSortChildrenOfContentModalData } from './sort-children-of-content-modal.token.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
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
				allowSorting: this._hasMorePages() === false,
			},
			{
				name: this.localize.term('content_createDate'),
				alias: 'createDate',
				allowSorting: this._hasMorePages() === false,
			},
		];
	}

	protected override async _createTableItems() {
		const itemDataResolver = (this.data as UmbSortChildrenOfContentModalData | undefined)?.itemDataResolver;

		this._tableItems = await Promise.all(
			this._children.map(async (treeItem) => {
				let name = treeItem.name;
				let icon = treeItem.icon ?? undefined;

				if (itemDataResolver) {
					const resolver = new itemDataResolver(this);
					resolver.setData(treeItem);
					name = (await resolver.getName()) || treeItem.name;
					icon = (await resolver.getIcon()) ?? icon;
					this.removeUmbController(resolver);
				}

				return {
					id: treeItem.unique,
					icon: icon ?? 'icon-document',
					data: [
						{
							columnAlias: 'name',
							value: name,
						},
						{
							columnAlias: 'createDate',
							value: this.localize.date(treeItem.createDate, this.#localizeDateOptions),
						},
					],
				};
			}),
		);
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
		'umb-sort-children-of-content-modal': UmbSortChildrenOfContentModalElement;
	}
}
