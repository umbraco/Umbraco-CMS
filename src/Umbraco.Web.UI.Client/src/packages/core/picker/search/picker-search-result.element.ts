import { UMB_PICKER_CONTEXT } from '../picker.context.token.js';
import type { UmbPickerContext } from '../picker.context.js';
import { UmbDefaultPickerSearchResultItemElement } from './result-item/default/default-picker-search-result-item.element.js';
import type { ManifestPickerSearchResultItem } from './result-item/picker-search-result-item.extension.js';
import { UmbDefaultPickerSearchResultItemContext } from './result-item/default/default-picker-search-result-item.context.js';
import { customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSearchRequestArgs, UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

type PickableFilterMethodType<T extends UmbEntityModel = UmbEntityModel> = (item: T) => boolean;

@customElement('umb-picker-search-result')
export class UmbPickerSearchResultElement extends UmbLitElement {
	@state()
	private _query?: UmbSearchRequestArgs;

	@state()
	private _searching: boolean = false;

	@state()
	private _items: UmbEntityModel[] = [];

	@state()
	private _isSearchable: boolean = false;

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
				'obsSearchable',
			);
			this.observe(this.#pickerContext?.search.query, (query) => (this._query = query), 'obsQuery');
			this.observe(
				this.#pickerContext?.search.searching,
				(query) => (this._searching = query ?? false),
				'obsSearching',
			);
			this.observe(this.#pickerContext?.search.resultItems, (items) => (this._items = items ?? []), 'obsResultItems');
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-search-result': UmbPickerSearchResultElement;
	}
}
