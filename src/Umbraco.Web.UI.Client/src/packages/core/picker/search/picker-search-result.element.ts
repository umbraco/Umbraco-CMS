import { UMB_PICKER_CONTEXT } from '../picker.context.token.js';
import type { UmbPickerContext } from '../picker.context.js';
import type { ManifestPickerSearchResultItem } from './result-item/picker-search-result-item.extension.js';
import { customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

const elementName = 'umb-picker-search-result';
@customElement(elementName)
export class UmbPickerSearchResultElement extends UmbLitElement {
	@state()
	_query?: UmbSearchRequestArgs;

	@state()
	_searching: boolean = false;

	@state()
	_items: UmbEntityModel[] = [];

	@state()
	_isSearchable: boolean = false;

	#pickerContext?: UmbPickerContext;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_CONTEXT, (context) => {
			this.#pickerContext = context;
			this.observe(this.#pickerContext.search.searchable, (isSearchable) => (this._isSearchable = isSearchable));
			this.observe(this.#pickerContext.search.query, (query) => (this._query = query));
			this.observe(this.#pickerContext.search.searching, (query) => (this._searching = query));
			this.observe(this.#pickerContext.search.resultItems, (items) => (this._items = items));
		});
	}

	override render() {
		if (!this._isSearchable) return nothing;

		if (this._query?.query && this._searching === false && this._items.length === 0) {
			return this.#renderEmptyResult();
		}

		return html`
			${repeat(
				this._items,
				(item) => item.unique,
				(item) => this.#renderResultItem(item),
			)}
		`;
	}

	#renderEmptyResult() {
		return html`<small>No result for <strong>"${this._query?.query}"</strong>.</small>`;
	}

	#renderResultItem(item: UmbEntityModel) {
		return html`
			<umb-extension-with-api-slot
				type="pickerSearchResultItem"
				.filter=${(manifest: ManifestPickerSearchResultItem) => manifest.forEntityTypes.includes(item.entityType)}
				.elementProps=${{ item }}></umb-extension-with-api-slot>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPickerSearchResultElement;
	}
}
