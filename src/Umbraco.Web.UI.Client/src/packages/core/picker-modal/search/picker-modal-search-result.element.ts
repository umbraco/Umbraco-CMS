import { UMB_PICKER_MODAL_CONTEXT } from '../picker-modal.context.token.js';
import type { UmbPickerModalContext } from '../picker-modal.context.js';
import { customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { ManifestPickerSearchResultItem } from '@umbraco-cms/backoffice/extension-registry';

const elementName = 'umb-picker-modal-search-result';
@customElement(elementName)
export class UmbPickerModalSearchResultElement extends UmbLitElement {
	@state()
	_query?: UmbSearchRequestArgs;

	@state()
	_searching: boolean = false;

	@state()
	_items: UmbEntityModel[] = [];

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

	override render() {
		if (this._query && this._searching === false && this._items.length === 0) {
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
		[elementName]: UmbPickerModalSearchResultElement;
	}
}
