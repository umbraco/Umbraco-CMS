import { UMB_PICKER_CONTEXT } from '../picker.context.token.js';
import type { UmbPickerContext } from '../picker.context.js';
import { UmbDefaultPickerSearchResultItemElement } from './result-item/default/default-picker-search-result-item.element.js';
import type { ManifestPickerSearchResultItem } from './result-item/picker-search-result-item.extension.js';
import { UmbDefaultPickerSearchResultItemContext } from './result-item/default/default-picker-search-result-item.context.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbSearchRequestArgs, UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

type PickableFilterMethodType<T extends UmbSearchResultItemModel = UmbSearchResultItemModel> = (item: T) => boolean;

@customElement('umb-picker-search-result')
export class UmbPickerSearchResultElement extends UmbLitElement {
	@state()
	private _query?: UmbSearchRequestArgs;

	@state()
	private _searching: boolean = false;

	@state()
	private _items: UmbItemModel[] = [];

	@state()
	private _isSearchable: boolean = false;

	@state()
	private _currentPage = 1;

	@state()
	private _totalPages = 1;

	@state()
	private _totalItems = 0;

	@property({ attribute: false })
	pickableFilter: PickableFilterMethodType = () => true;

	#pickerContext?: UmbPickerContext;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_CONTEXT, (context) => {
			this.#pickerContext = context;

			this.observe(
				this.#pickerContext?.search.searchable,
				(isSearchable) => (this._isSearchable = isSearchable ?? false),
				null,
			);

			this.observe(this.#pickerContext?.search.query, (query) => (this._query = query), null);

			this.observe(this.#pickerContext?.search.searching, (query) => (this._searching = query ?? false), null);

			this.observe(this.#pickerContext?.search.resultItems, (items) => (this._items = items ?? []), null);

			this.observe(
				this.#pickerContext?.search.pagination.currentPage,
				(currentPage) => (this._currentPage = currentPage ?? 1),
				null,
			);

			this.observe(
				this.#pickerContext?.search.pagination.totalPages,
				(totalPages) => (this._totalPages = totalPages ?? 1),
				null,
			);

			this.observe(
				this.#pickerContext?.search.resultTotalItems,
				(totalItems) => (this._totalItems = totalItems ?? 0),
				null,
			);
		});
	}

	override render() {
		if (!this._isSearchable) return nothing;

		if (this._query?.query && this._searching === false && this._items.length === 0) {
			return this.#renderEmptyResult();
		}

		if (this._items.length === 0) {
			return;
		}

		return html`
			<uui-box id="result-container">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderResultItem(item),
				)}
				${this.#renderPagination()}
			</uui-box>
		`;
	}

	#onPageChange(event: UUIPaginationEvent) {
		this.#pickerContext?.search.pagination.setCurrentPageNumber(event.target.current);
		this.#pickerContext?.search.search();
	}

	#renderPagination() {
		// Don't show pagination if all items are loaded or there's only one page
		if (this._items.length === this._totalItems || this._totalPages <= 1) {
			return nothing;
		}

		return html`<uui-pagination
			.current=${this._currentPage}
			.total=${this._totalPages}
			@change=${this.#onPageChange}></uui-pagination>`;
	}

	#renderEmptyResult() {
		return html`<uui-box>
			<small>No result for <strong>"${this._query?.query}"</strong>.</small>
		</uui-box>`;
	}

	#renderResultItem(item: UmbSearchResultItemModel) {
		return html`
			<umb-extension-with-api-slot
				type="pickerSearchResultItem"
				.filter=${(manifest: ManifestPickerSearchResultItem) => manifest.forEntityTypes.includes(item.entityType)}
				.elementProps=${{
					item,
					disabled: this.pickableFilter ? !this.pickableFilter(item) : undefined,
				}}
				.fallbackRenderMethod=${() => this.#renderFallbackResultItem(item)}></umb-extension-with-api-slot>
		`;
	}

	#renderFallbackResultItem(item: UmbSearchResultItemModel) {
		const element = new UmbDefaultPickerSearchResultItemElement();
		element.item = item;
		element.disabled = this.pickableFilter ? !this.pickableFilter(item) : undefined;
		new UmbDefaultPickerSearchResultItemContext(element);
		return element;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
			}

			umb-extension-with-api-slot {
				display: block;
				margin-bottom: var(--uui-size-3);

				&:last-of-type {
					margin-bottom: 0;
				}
			}

			uui-pagination {
				display: block;
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-search-result': UmbPickerSearchResultElement;
	}
}
