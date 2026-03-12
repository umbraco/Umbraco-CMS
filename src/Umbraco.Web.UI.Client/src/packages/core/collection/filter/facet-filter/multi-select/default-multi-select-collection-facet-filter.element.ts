import type { UmbSelectOption } from '../collection-facet-filter-api.interface.js';
import type { ManifestCollectionFacetFilter } from '../collection-facet-filter.extension.js';
import type { UmbDefaultMultiSelectCollectionFacetFilterApi } from './default-multi-select-collection-facet-filter.api.js';
import type { UmbDatalistItemModel } from '@umbraco-cms/backoffice/datalist-data-source';
import { css, customElement, html, ifDefined, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-default-multi-select-collection-facet-filter')
export class UmbDefaultMultiSelectCollectionFacetFilterElement extends UmbLitElement {
	public manifest?: ManifestCollectionFacetFilter;

	@state()
	private _options: Array<UmbSelectOption> = [];

	@state()
	private _value: Array<string> = [];

	@state()
	private _valueItems: Array<UmbDatalistItemModel> = [];

	@state()
	private _hasMore = false;

	private _api?: UmbDefaultMultiSelectCollectionFacetFilterApi | undefined;
	public get api(): UmbDefaultMultiSelectCollectionFacetFilterApi | undefined {
		return this._api;
	}
	public set api(api: UmbDefaultMultiSelectCollectionFacetFilterApi | undefined) {
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

	#onChange(event: Event) {
		const target = event.currentTarget as HTMLInputElement;
		const item = this._options.find((option) => option.value === target.value);

		if (!item) return;

		let value;
		if (target.checked) {
			value = [...this._value, item.value];
		} else {
			value = this._value.filter((group) => group !== item.value);
		}

		this._api?.setValue(value);
	}

	#onLoadMore() {
		this._api?.loadMoreOptions();
	}

	#getUserGroupFilterLabel() {
		const length = this._value.length;
		const max = 2;
		if (length === 0) return this.localize.term('general_all');

		const labels = this._value
			.slice(0, max)
			.map((unique) => this._valueItems.find((i) => i.unique === unique)?.name ?? unique);

		return labels.join(', ') + (length > max ? ' + ' + (length - max) : '');
	}

	protected override render() {
		return html`
			<div class="filter">
				<span class="label">${this.manifest?.meta.label}</span>
				<uui-button popovertarget="collection-multi-select-filter-popover" label="Multi Select" compact>
					<b>${this.#getUserGroupFilterLabel()}</b>
				</uui-button>
				<uui-popover-container id="collection-multi-select-filter-popover" placement="bottom">
					<umb-popover-layout>
						<div class="filter-dropdown">
							${repeat(
								this._options,
								(group) => group.value,
								(group) => html`
									<uui-checkbox
										label=${ifDefined(group.label)}
										value=${ifDefined(group.value)}
										@change=${this.#onChange}
										.checked=${this._value.includes(group.value)}></uui-checkbox>
								`,
							)}
							${this._hasMore
								? html`<uui-button label=${this.localize.term('general_loadMore')} @click=${this.#onLoadMore} compact>
										${this.localize.term('general_loadMore')}
									</uui-button>`
								: nothing}
						</div>
					</umb-popover-layout>
				</uui-popover-container>
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
			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
				padding: var(--uui-size-space-3);
			}
			.label {
				font-weight: 600;
				font-size: var(--uui-size-4);
			}
		`,
	];
}

export { UmbDefaultMultiSelectCollectionFacetFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-multi-select-collection-facet-filter': UmbDefaultMultiSelectCollectionFacetFilterElement;
	}
}
