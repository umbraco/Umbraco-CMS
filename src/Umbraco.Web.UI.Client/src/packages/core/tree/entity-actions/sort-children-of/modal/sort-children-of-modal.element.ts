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

	#pagination = new UmbPaginationManager();
	#sortedUniques = new Set<string>();
	#sorter?: UmbSorterController<UmbTreeItemModel>;

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

	protected async firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): Promise<void> {
		super.firstUpdated(_changedProperties);
		this.#requestChildren();
	}

	async #requestChildren() {
		if (!this.data?.unique === undefined) throw new Error('unique is required');
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
			itemSelector: 'uui-ref-node',
			containerSelector: 'uui-ref-list',
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

	render() {
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
			<uui-ref-list>
				${repeat(
					this._children,
					(child) => child.unique,
					(child) => this.#renderChild(child),
				)}
			</uui-ref-list>

			${this._currentPage < this._totalPages
				? html`
						<uui-button id="loadMoreButton" look="secondary" @click=${this.#onLoadMore}
							>Load More (${this._currentPage}/${this._totalPages})</uui-button
						>
					`
				: nothing}
		`;
	}

	#renderChild(item: UmbTreeItemModel) {
		return html`<uui-ref-node .name=${item.name} data-unique=${item.unique}></uui-ref-node>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#loadMoreButton {
				width: 100%;
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
