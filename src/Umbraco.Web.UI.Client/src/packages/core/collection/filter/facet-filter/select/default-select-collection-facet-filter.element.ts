import type { UmbCollectionFacetFilterApi, UmbSelectOption } from '../collection-facet-filter-api.interface.js';
import type { ManifestCollectionFacetFilter } from '../collection-facet-filter.extension.js';
import { css, customElement, html, repeat, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDefaultSelectCollectionFacetFilterApi } from './default-select-collection-facet-filter.api.js';
import type { UmbDatalistItemModel } from '@umbraco-cms/backoffice/datalist-data-source';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-default-select-collection-facet-filter')
export class UmbDefaultSelectCollectionFacetFilterElement extends UmbLitElement {
	public manifest?: ManifestCollectionFacetFilter;

	@state()
	private _options: Array<UmbSelectOption> = [];

	@state()
	private _value: Array<string> = [];

	@state()
	private _valueItems: Array<UmbDatalistItemModel> = [];

	@state()
	private _hasMore = false;

	_api?: UmbDefaultSelectCollectionFacetFilterApi | undefined;
	public get api(): UmbDefaultSelectCollectionFacetFilterApi | undefined {
		return this._api;
	}
	public set api(api: UmbDefaultSelectCollectionFacetFilterApi | undefined) {
		this._api = api;
		this.observe(api?.options, (options) => (this._options = options ?? []));
		this.observe(api?.value, (value) => (this._value = value ?? []));
		this.observe(api?.valueItems, (items) => (this._valueItems = items ?? []));

		if (api) {
			this.observe(
				observeMultiple([api.pagination.currentPage, api.pagination.totalPages]),
				([currentPage, totalPages]) => (this._hasMore = currentPage < totalPages),
			);
		}

		this._api?.loadOptions();
	}

	#onSelect(event: Event) {
		const target = event.target as HTMLInputElement;
		this._api?.setValue([target.value]);
	}

	#onLoadMore() {
		this._api?.loadMoreOptions();
	}

	protected override render() {
		return html`
			<div class="filter">
				<span class="label">${this.manifest?.meta?.label ?? 'Filter'}:</span>
				<uui-combobox value=${this._value[0] ?? ''} @change=${this.#onSelect} placeholder="Placeholder">
					<uui-combobox-list>
						${repeat(
							this._options,
							(option) => option.value,
							(option) => html`
								<uui-combobox-list-option value=${option.value}> ${option.label} </uui-combobox-list-option>
							`,
						)}
						${this._hasMore
							? html`<uui-button label=${this.localize.term('general_loadMore')} @click=${this.#onLoadMore} compact>
									${this.localize.term('general_loadMore')}
								</uui-button>`
							: nothing}
					</uui-combobox-list>
				</uui-combobox>
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				border-top: 1px solid var(--uui-color-border);
				padding-top: var(--uui-size-space-5);
			}
			.filter {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
			.label {
				font-weight: 600;
				font-size: var(--uui-size-4);
			}
		`,
	];
}

export { UmbDefaultSelectCollectionFacetFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-select-collection-facet-filter': UmbDefaultSelectCollectionFacetFilterElement;
	}
}
