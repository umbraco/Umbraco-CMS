import type { UmbSortChildrenOfRepository, UmbTreeItemModel } from '../../../types.js';
import type { UmbSortChildrenByFieldOption } from '../types.js';
import type { UmbTreeRepository } from '../../../data/index.js';
import type { UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue } from './sort-children-of-modal.token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UUIButtonState, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import type {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableElement,
	UmbTableItem,
	UmbTableOrderedEvent,
	UmbTableSortedEvent,
} from '@umbraco-cms/backoffice/components';

const UMB_SORT_TAB_INDIVIDUALLY = 'individually';
const UMB_SORT_TAB_BY_FIELD = 'byField';

@customElement('umb-sort-children-of-modal')
export class UmbSortChildrenOfModalElement<
	TreeItemModelType extends UmbTreeItemModel = UmbTreeItemModel,
> extends UmbModalBaseElement<UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue> {
	@state()
	protected _children: Array<TreeItemModelType> = [];

	@state()
	private _currentPage = 1;

	@state()
	private _totalPages = 1;

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

	@state()
	private _loading = false;

	@state()
	private _submitButtonState?: UUIButtonState;

	@state()
	private _activeTab: string = UMB_SORT_TAB_INDIVIDUALLY;

	@state()
	private _supportsSortByField = false;

	@state()
	private _sortByFieldOptions: Array<UmbSortChildrenByFieldOption> = [];

	@state()
	private _selectedField?: string;

	@state()
	private _selectedDirection: DirectionModel = DirectionModel.ASCENDING;

	protected _sortedUniques = new Set<string>();

	#pagination = new UmbPaginationManager();

	#sortChildrenOfRepository?: UmbSortChildrenOfRepository;

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

	/**
	 * The fields offered by the "Sort by field" view. Returns an empty array by default, which hides the view.
	 * Override in an entity-specific subclass to enable server-side sorting by field.
	 * @returns {Array<UmbSortChildrenByFieldOption>} the available fields
	 */
	protected _getSortByFieldOptions(): Array<UmbSortChildrenByFieldOption> {
		return [];
	}

	/**
	 * The culture to sort by when sorting by field. Undefined for invariant sorting.
	 * @returns {string | undefined} the culture
	 */
	protected _getSortCulture(): string | undefined {
		return undefined;
	}

	protected override async firstUpdated(
		_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>,
	): Promise<void> {
		super.firstUpdated(_changedProperties);
		await this.#requestChildren();
		this._setTableColumns();
		await this.#initSortByField();
	}

	async #initSortByField() {
		this._sortByFieldOptions = this._getSortByFieldOptions();
		this._selectedField = this._sortByFieldOptions[0]?.value;

		if (this._sortByFieldOptions.length === 0) {
			this._supportsSortByField = false;
			return;
		}

		try {
			const repository = await this.#getSortChildrenOfRepository();
			this._supportsSortByField = typeof repository.sortChildrenOfByField === 'function';
		} catch {
			this._supportsSortByField = false;
		}
	}

	async #getSortChildrenOfRepository(): Promise<UmbSortChildrenOfRepository> {
		if (!this.#sortChildrenOfRepository) {
			if (!this.data?.sortChildrenOfRepositoryAlias) throw new Error('sortChildrenOfRepositoryAlias is required');
			this.#sortChildrenOfRepository = await createExtensionApiByAlias<UmbSortChildrenOfRepository>(
				this,
				this.data.sortChildrenOfRepositoryAlias,
			);
		}
		return this.#sortChildrenOfRepository;
	}

	async #requestChildren() {
		if (this.data?.unique === undefined) throw new Error('unique is required');
		if (!this.data?.treeRepositoryAlias) throw new Error('treeRepositoryAlias is required');

		this._loading = true;
		try {
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
				await this._createTableItems();
			}
		} finally {
			this._loading = false;
		}
	}

	protected _createTableItems(): void | Promise<void> {
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
		if (this._loading) return;
		if (this._currentPage >= this._totalPages) return;
		this.#pagination.setCurrentPageNumber(this._currentPage + 1);
		this.#requestChildren();
	}

	async #onSubmit(event: PointerEvent) {
		if (this._submitButtonState === 'waiting') return;
		event?.stopPropagation();

		this._submitButtonState = 'waiting';

		try {
			const repository = await this.#getSortChildrenOfRepository();
			const { error } =
				this._supportsSortByField && this._activeTab === UMB_SORT_TAB_BY_FIELD
					? await this.#sortByField(repository)
					: await repository.sortChildrenOf({
							unique: this.data!.unique,
							sorting: this.#getSortOrderOfSortedItems(),
						});

			if (!error) {
				this._submitButtonState = 'success';
				this._submitModal();
			} else {
				this._submitButtonState = 'failed';
			}
		} catch (error) {
			this._submitButtonState = 'failed';
			throw error;
		}
	}

	#sortByField(repository: UmbSortChildrenOfRepository) {
		if (!repository.sortChildrenOfByField) throw new Error('sortChildrenOfByField is not supported');
		return repository.sortChildrenOfByField({
			unique: this.data!.unique,
			field: this._selectedField!,
			direction: this._selectedDirection,
			culture: this._getSortCulture(),
		});
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

	#onFieldChange(event: UUISelectEvent) {
		this._selectedField = event.target.value as string;
	}

	#onDirectionChange(event: UUISelectEvent) {
		this._selectedDirection = event.target.value as DirectionModel;
	}

	override render() {
		const hasChildren = this._children.length > 0;
		return html`
			<umb-body-layout headline=${this.localize.term('actions_sort')}>
				${hasChildren && this._supportsSortByField ? this.#renderTabs() : nothing}
				${hasChildren ? this.#renderActiveView() : this.#renderEmptyState()}
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click="${this._rejectModal}"></uui-button>
				<uui-button
					slot="actions"
					color="positive"
					look="primary"
					label=${this.localize.term('general_sort')}
					.state=${this._submitButtonState}
					@click=${this.#onSubmit}></uui-button>
			</umb-body-layout>
		`;
	}

	#renderTabs() {
		return html`
			<uui-tab-group slot="navigation">
				<uui-tab
					label=${this.localize.term('sort_sortIndividuallyHeadline')}
					data-mark="sort-children-of-modal:tab-individually"
					?active=${this._activeTab === UMB_SORT_TAB_INDIVIDUALLY}
					@click=${() => (this._activeTab = UMB_SORT_TAB_INDIVIDUALLY)}>
					<umb-icon slot="icon" name="icon-grip"></umb-icon>
					${this.localize.term('sort_sortIndividuallyHeadline')}
				</uui-tab>
				<uui-tab
					label=${this.localize.term('sort_sortByFieldHeadline')}
					data-mark="sort-children-of-modal:tab-by-field"
					?active=${this._activeTab === UMB_SORT_TAB_BY_FIELD}
					@click=${() => (this._activeTab = UMB_SORT_TAB_BY_FIELD)}>
					<umb-icon slot="icon" name="icon-ordered-list"></umb-icon>
					${this.localize.term('sort_sortByFieldHeadline')}
				</uui-tab>
			</uui-tab-group>
		`;
	}

	#renderActiveView() {
		if (this._supportsSortByField && this._activeTab === UMB_SORT_TAB_BY_FIELD) {
			return this.#renderSortByFieldView();
		}
		return this.#renderIndividuallyView();
	}

	#renderEmptyState() {
		return html`
			<uui-label><umb-localize key="sort_sortEmptyState">This node has no child nodes to sort</umb-localize></uui-label>
		`;
	}

	#renderSortByFieldView() {
		const fieldOptions = this._sortByFieldOptions.map((option) => ({
			name: option.label,
			value: option.value,
			selected: option.value === this._selectedField,
		}));

		const directionOptions = [
			{
				name: this.localize.term('sort_sortByFieldAscending'),
				value: DirectionModel.ASCENDING,
				selected: this._selectedDirection === DirectionModel.ASCENDING,
			},
			{
				name: this.localize.term('sort_sortByFieldDescending'),
				value: DirectionModel.DESCENDING,
				selected: this._selectedDirection === DirectionModel.DESCENDING,
			},
		];

		return html`
			<uui-box>
				<div id="sort-by-field">
					<span><umb-localize key="sort_sortByFieldSentence">Sort all children by</umb-localize></span>
					<uui-select
						label=${this.localize.term('sort_sortByFieldHeadline')}
						.options=${fieldOptions}
						@change=${this.#onFieldChange}></uui-select>
					<uui-select
						label=${this.localize.term('sort_sortByFieldDirectionLabel')}
						.options=${directionOptions}
						@change=${this.#onDirectionChange}></uui-select>
				</div>
			</uui-box>
		`;
	}

	#renderIndividuallyView() {
		return html`
			<umb-table
				.config=${this._tableConfig}
				.columns=${this._tableColumns}
				.items=${this._tableItems}
				.sortable=${this._sortable}
				@sorted=${this.#onSorted}
				@ordered=${this.#onOrdered}>
			</umb-table>

			${this._hasMorePages()
				? html`
						<uui-button
							id="loadMoreButton"
							look="placeholder"
							?disabled=${this._loading}
							@click=${this._onLoadMore}>
							<umb-localize key="actions_loadMore">Load more</umb-localize> (${this._currentPage}/${this._totalPages})
						</uui-button>
					`
				: nothing}
		`;
	}

	static override styles = [
		css`
			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}

			#sort-by-field {
				display: flex;
				gap: var(--uui-size-space-3);
				align-items: center;
				flex-wrap: wrap;
			}

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
		'umb-sort-children-of-modal': UmbSortChildrenOfModalElement;
	}
}
