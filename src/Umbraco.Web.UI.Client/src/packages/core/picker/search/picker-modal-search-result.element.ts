import { UMB_PICKER_MODAL_CONTEXT } from '../picker-modal.context.token.js';
import type { UmbPickerModalContext } from '../picker-modal.context.js';
import { customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-picker-modal-search-result';
@customElement(elementName)
export class UmbPickerModalSearchResultElement extends UmbLitElement {
	@state()
	_query: unknown;

	@state()
	_searching: boolean = false;

	@state()
	_items: unknown[] = [];

	#pickerModalContext?: UmbPickerModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_MODAL_CONTEXT, (context) => {
			this.#pickerModalContext = context;

			this.observe(this.#pickerModalContext.search.query, (query) => {
				this._query = query;
			});

			this.observe(this.#pickerModalContext.search.searching, (query) => {
				this._searching = query;
			});

			this.observe(this.#pickerModalContext.search.resultItems, (items) => {
				this._items = items;
			});
		});
	}

	override render(): unknown {
		if (this._query && this._searching === false && this._items.length === 0) {
			return this.#renderEmptyResult();
		}

		return html`
			${repeat(
				this._items,
				(item: any) => item.unique,
				(item: any) => this.#renderResultItem(item),
			)}
		`;
	}

	#renderEmptyResult() {
		return html`<small>No result for <strong>"${this._query}"</strong>.</small>`;
	}

	#renderResultItem(item: any) {
		return html`
			<umb-picker-search-result-item .props=${{ item }} .entityType=${item.entityType}></umb-picker-search-result-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPickerModalSearchResultElement;
	}
}
