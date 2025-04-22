import type { UmbSortChildrenOfRepository, UmbTreeItemModel } from '../../../types.js';
import type { UmbTreeRepository } from '../../../data/index.js';
import type { UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue } from './sort-children-of-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, css, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSortedEvent,
} from '@umbraco-cms/backoffice/components';

const elementName = 'umb-sort-children-of-modal';

@customElement(elementName)
export class UmbSortChildrenOfModalElement<
	TreeItemModelType extends UmbTreeItemModel = UmbTreeItemModel,
> extends UmbModalBaseElement<UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue> {
	@state()
	protected _children: Array<TreeItemModelType> = [];

	@state()
	_currentPage = 1;

	@state()
	_totalPages = 1;

	@state()
	protected _tableColumns: Array<UmbTableColumn> = [];

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	protected _tableItems: Array<UmbTableItem> = [];

	@state()
	private _sortable = false;

	protected _sortedUniques = new Set<string>();

	#pagination = new UmbPaginationManager();

	constructor() {
		super();
		this.#pagination.setPageSize(50);

		this.observe(
			observeMultiple([this.#pagination.currentPage, this.#pagination.totalPages]),
			([currentPage, totalPages]) => {
				this._currentPage = currentPage;
				this._totalPages = totalPages;
			},
			'umbPaginationObserver',
		);
	}

	protected _setTableColumns() {
		this._tableColumns = [
			{
				name: this.localize.term('general_name'),
				alias: 'name',
				allowSorting: true,
			},
		];
	}

	protected override async firstUpdated(
		_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>,
	): Promise<void> {
		super.firstUpdated(_changedProperties);
		await this.#requestChildren();
		this._setTableColumns();
	}

	async #requestChildren() {
		if (this.data?.unique === undefined) throw new Error('unique is required');
		if (!this.data?.treeRepositoryAlias) throw new Error('treeRepositoryAlias is required');

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<TreeItemModelType>>(
			this,
			this.data.treeRepositoryAlias,
		);

		const { data } = await treeRepository.requestTreeItemsOf({
			parent: {
				unique: this.data.unique,
				entityType: this.data.entityType,
			},
			skip: this.#pagination.getSkip(),
			take: this.#pagination.getPageSize(),
		});

		if (data) {
			this._children = [...this._children, ...data.items];
			this.#pagination.setTotalItems(data.total);
			this._sortable = this._children.length > 0;
			this._createTableItems();
		}
	}

	protected _createTableItems() {
		this._tableItems = this._children.map((treeItem) => {
			return {
				id: treeItem.unique,
				icon: 'icon-globe',
				data: [
					{
						columnAlias: 'name',
						value: html`${treeItem.name}`,
					},
				],
			};
		});
	}

	protected _hasMorePages() {
		return this._currentPage < this._totalPages;
	}

	protected _onLoadMore(event: PointerEvent) {
		event.stopPropagation();
		if (this._currentPage >= this._totalPages) return;
		this.#pagination.setCurrentPageNumber(this._currentPage + 1);
		this.#requestChildren();
	}

	async #onSubmit(event: PointerEvent) {
		event?.stopPropagation();
		if (!this.data?.sortChildrenOfRepositoryAlias) throw new Error('sortChildrenOfRepositoryAlias is required');

		const sortChildrenOfRepository = await createExtensionApiByAlias<UmbSortChildrenOfRepository>(
			this,
			this.data.sortChildrenOfRepositoryAlias,
		);

		const { error } = await sortChildrenOfRepository.sortChildrenOf({
			unique: this.data.unique,
			sorting: this.#getSortOrderOfSortedItems(),
		});

		if (!error) {
			this._submitModal();
		}
	}

	#getSortOrderOfSortedItems() {
		const sorting = [];

		// get the new sort order from the sorted uniques
		for (const value of this._sortedUniques) {
			const index = this._tableItems.findIndex((tableItem) => tableItem.id === value);
			if (index !== -1) {
				const entry = {
					unique: value,
					sortOrder: index,
				};

				sorting.push(entry);
			}
		}

		return sorting;
	}

	#onSorted(event: UmbTableSortedEvent) {
		event.stopPropagation();
		const sortedId = event.getItemId();
		this._sortedUniques.add(sortedId);
		const target = event.target as UmbTableElement;
		const items = target.items;
		this._tableItems = items;
	}

	#onOrdered(event: UmbTableOrderedEvent) {
		event.stopPropagation();
		const target = event.target as UmbTableElement;
		const orderingColumn = target.orderingColumn;
		const orderingDesc = target.orderingDesc;

		this._tableItems = [...this._tableItems].sort((a, b) => {
			const aColumn = a.data.find((column) => column.columnAlias === orderingColumn);
			const bColumn = b.data.find((column) => column.columnAlias === orderingColumn);
			return this._sortCompare(orderingColumn, aColumn?.value, bColumn?.value);
		});

		if (orderingDesc) {
			this._tableItems.reverse();
		}

		this._sortedUniques.clear();
		this._tableItems.map((tableItem) => tableItem.id).forEach((u) => this._sortedUniques.add(u));
	}

	protected _sortCompare(columnAlias: string, valueA: unknown, valueB: unknown): number {
		if (columnAlias === 'name') {
			return (valueA as string).localeCompare(valueB as string);
		}
		return 0;
	}

	override render() {
		return html`
			<umb-body-layout headline=${'Sort Children'}>
				${this._children.length === 0 ? this.#renderEmptyState() : this.#renderTable()}
				<uui-button slot="actions" label="Cancel" @click="${this._rejectModal}"></uui-button>
				<uui-button slot="actions" color="positive" look="primary" label="Sort" @click=${this.#onSubmit}></uui-button>
			</umb-body-layout>
		`;
	}

	#renderEmptyState() {
		return html`<uui-label>There are no children</uui-label>`;
	}

	#renderTable() {
		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.sortable=${this._sortable}
				@sorted=${this.#onSorted}
				@ordered=${this.#onOrdered}></umb-table>

			${this._hasMorePages()
				? html`
						<uui-button id="loadMoreButton" look="placeholder" @click=${this._onLoadMore}
							>Load more (${this._currentPage}/${this._totalPages})</uui-button
						>
					`
				: nothing}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#loadMoreButton {
				width: 100%;
				margin-top: var(--uui-size-space-3);
			}
		`,
	];
}

export { UmbSortChildrenOfModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbSortChildrenOfModalElement;
	}
}
