import type { UmbContentTreeItemModel } from '../../types.js';
import type { UmbSortChildrenOfContentModalData } from './sort-children-of-content-modal.token.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbSortChildrenOfModalElement } from '@umbraco-cms/backoffice/tree';
import type { UmbSortChildrenByFieldOption } from '@umbraco-cms/backoffice/tree';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { ContentSortFieldModel } from '@umbraco-cms/backoffice/external/backend-api';

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
		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appCulture = instance?.getAppCulture();
		});
	}

	protected override _getSortCulture(): string | undefined {
		return this.#appCulture;
	}

	protected override _getSortByFieldOptions(): Array<UmbSortChildrenByFieldOption> {
		return [
			{ value: ContentSortFieldModel.NAME, label: this.localize.term('sort_sortByFieldNameOption') },
			{ value: ContentSortFieldModel.CREATE_DATE, label: this.localize.term('sort_sortByFieldCreateDateOption') },
			{ value: ContentSortFieldModel.UPDATE_DATE, label: this.localize.term('sort_sortByFieldUpdateDateOption') },
		];
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
