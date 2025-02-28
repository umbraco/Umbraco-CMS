import type { UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue } from './sort-children-of-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, css, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbTreeRepository, UmbTreeItemModel, UmbSortChildrenOfRepository } from '@umbraco-cms/backoffice/tree';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbDocumentTreeItemModel } from '@umbraco-cms/backoffice/document';
import type { UmbMediaTreeItemModel } from '@umbraco-cms/backoffice/media';

const elementName = 'umb-sort-children-of-modal';

@customElement(elementName)
export class UmbSortChildrenOfModalElement extends UmbModalBaseElement<
	UmbSortChildrenOfModalData,
	UmbSortChildrenOfModalValue
> {
	@state()
	_children: Array<UmbTreeItemModel> = [];

	@state()
	_currentPage = 1;

	@state()
	_totalPages = 1;

	@state()
	_isSorting: boolean = false;

	#hasMorePages() {
		return this._currentPage < this._totalPages;
	}

	#pagination = new UmbPaginationManager();
	#sortedUniques = new Set<string>();
	#sorter?: UmbSorterController<UmbTreeItemModel>;

	#sortBy: string = '';
	#sortDirection: string = '';

	#localizeDateOptions: Intl.DateTimeFormatOptions = {
		day: 'numeric',
		month: 'short',
		year: 'numeric',
		hour: 'numeric',
		minute: '2-digit',
	};

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

	protected override async firstUpdated(
		_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>,
	): Promise<void> {
		super.firstUpdated(_changedProperties);
		this.#requestChildren();
	}

	async #requestChildren() {
		if (this.data?.unique === undefined) throw new Error('unique is required');
		if (!this.data?.treeRepositoryAlias) throw new Error('treeRepositoryAlias is required');

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<UmbTreeItemModel>>(
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

			if (data.total > 0) {
				this.#initSorter();
				this.#sorter?.setModel(this._children);
			}
		}
	}

	#initSorter() {
		if (this.#sorter) return;

		this.#sorter = new UmbSorterController<UmbTreeItemModel>(this, {
			getUniqueOfElement: (element) => {
				return element.dataset.unique;
			},
			getUniqueOfModel: (modelEntry) => {
				return modelEntry.unique;
			},
			identifier: 'Umb.SorterIdentifier.SortChildrenOfModal',
			itemSelector: 'uui-table-row',
			containerSelector: 'uui-table',
			onChange: ({ model }) => {
				const oldValue = this._children;
				this._children = model;
				this.requestUpdate('_children', oldValue);
			},
			onEnd: ({ item }) => {
				this.#sortedUniques.add(item.unique);
			},
		});
	}

	#onLoadMore(event: PointerEvent) {
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

	#onSortChildrenBy(key: string) {
		if (this._isSorting) {
			return;
		}

		this._isSorting = true;

		const oldValue = this._children;

		// If switching column, revert to ascending sort. Otherwise switch from whatever was previously selected.
		if (this.#sortBy !== key) {
			this.#sortDirection = 'asc';
		} else {
			this.#sortDirection = this.#sortDirection === 'asc' ? 'desc' : 'asc';
		}

		// Sort by the new column.
		this.#sortBy = key;
		this._children = [...this._children].sort((a, b) => {
			switch (key) {
				case 'name':
					return a.name.localeCompare(b.name);
				case 'createDate':
					return Date.parse(this.#getCreateDate(a)) - Date.parse(this.#getCreateDate(b));
				default:
					return 0;
			}
		});

		// Reverse the order if sorting descending.
		if (this.#sortDirection === 'desc') {
			this._children.reverse();
		}

		this.#sortedUniques.clear();
		this._children.map((c) => c.unique).forEach((u) => this.#sortedUniques.add(u));

		this.requestUpdate('_children', oldValue);

		this._isSorting = false;
	}

	#getSortOrderOfSortedItems() {
		const sorting = [];

		// get the new sort order from the sorted uniques
		for (const value of this.#sortedUniques) {
			const index = this._children.findIndex((child) => child.unique === value);
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

	#getCreateDate(item: UmbTreeItemModel): string {
		let date = '';
		const itemAsDocumentTreeItemModel = item as UmbDocumentTreeItemModel;
		if (itemAsDocumentTreeItemModel) {
			date = itemAsDocumentTreeItemModel.createDate;
		} else {
			const itemAsMediaTreeItemModel = item as UmbMediaTreeItemModel;
			if (itemAsMediaTreeItemModel) {
				date = itemAsMediaTreeItemModel.createDate;
			}
		}

		return date;
	}

	override render() {
		return html`
			<umb-body-layout headline=${'Sort Children'}>
				<uui-box>${this.#renderChildren()}</uui-box>
				<uui-button slot="actions" label="Cancel" @click="${this._rejectModal}"></uui-button>
				<uui-button slot="actions" color="positive" look="primary" label="Sort" @click=${this.#onSubmit}></uui-button>
			</umb-body-layout>
		`;
	}

	#renderChildren() {
		if (this._children.length === 0) return html`<uui-label>There are no children</uui-label>`;
		return html`
			<uui-table>
				<uui-table-head>
					<uui-table-head-cell></uui-table-head-cell>
					${this.#renderHeaderCell('name', 'general_name')}
					${this.#renderHeaderCell('createDate', 'content_createDate')}
				</uui-table-head>
				${this._isSorting
					? html`
							<uui-table-row>
								<uui-table-cell></uui-table-cell>
								<uui-table-cell><uui-loader-circle></uui-loader-circle></uui-table-cell>
								<uui-table-cell></uui-table-cell>
							</uui-table-row>
						`
					: nothing}
				${repeat(
					this._children,
					(child) => child.unique,
					(child) => this.#renderChild(child),
				)}
			</uui-table>

			${this.#hasMorePages()
				? html`
						<uui-button id="loadMoreButton" look="secondary" @click=${this.#onLoadMore}
							>Load More (${this._currentPage}/${this._totalPages})</uui-button
						>
					`
				: nothing}
		`;
	}

	#renderHeaderCell(key: string, labelKey: string) {
		// Only provide buttons for sorting via the column headers if all pages have been loaded.
		return html` <uui-table-head-cell>
			${this.#hasMorePages()
				? html` <span>${this.localize.term(labelKey)}</span> `
				: html`
						<button @click=${() => this.#onSortChildrenBy(key)}>
							${this.localize.term(labelKey)}
							<uui-symbol-sort
								?active=${this.#sortBy === key}
								?descending=${this.#sortDirection === 'desc'}></uui-symbol-sort>
						</button>
					`}
		</uui-table-head-cell>`;
	}

	#renderChild(item: UmbTreeItemModel) {
		// TODO: find a way to get the icon for the item. We do not have the icon in the tree item model.
		return html` <uui-table-row id="content-node" data-unique=${item.unique} class="${this._isSorting ? 'hidden' : ''}">
			<uui-table-cell><umb-icon name="icon-drag-vertical"></umb-icon></uui-table-cell>
			<uui-table-cell>${item.name}</uui-table-cell>
			<uui-table-cell>${this.#renderCreateDate(item)}</uui-table-cell>
		</uui-table-row>`;
	}

	#renderCreateDate(item: UmbTreeItemModel) {
		const date = this.#getCreateDate(item);
		if (date.length === 0) {
			return nothing;
		}

		return html`<umb-localize-date date="${date}" .options=${this.#localizeDateOptions}></umb-localize-date>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#loadMoreButton {
				width: 100%;
			}

			uui-table-cell {
				padding: var(--uui-size-space-2) var(--uui-size-space-5);
			}

			uui-table-head-cell {
				padding: 0 var(--uui-size-space-5);
			}

			uui-table-head-cell button {
				background-color: transparent;
				color: inherit;
				border: none;
				cursor: pointer;
				font-weight: inherit;
				font-size: inherit;
				display: inline-flex;
				align-items: center;
				justify-content: space-between;
				width: 100%;
				padding: var(--uui-size-5) var(--uui-size-1);
			}

			uui-table-row.hidden {
				visibility: hidden;
			}

			uui-table-row[id='content-node']:hover {
				cursor: grab;
			}

			uui-icon[name='icon-navigation'] {
				cursor: hand;
			}

			uui-box {
				--uui-box-default-padding: 0;
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
