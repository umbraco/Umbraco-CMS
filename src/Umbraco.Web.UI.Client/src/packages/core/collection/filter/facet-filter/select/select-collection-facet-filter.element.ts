import type { ManifestCollectionFacetFilter } from '../collection-facet-filter.extension.js';
import type { UmbSelectOption, MetaCollectionFacetFilterSelect } from './types.js';
import type { UmbSelectCollectionFacetFilterApi } from './select-collection-facet-filter.api.js';
import type { UmbDatalistItemModel } from '@umbraco-cms/backoffice/datalist-data-source';
import { css, customElement, html, ifDefined, repeat, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

const INLINE_THRESHOLD = 6;

@customElement('umb-select-collection-facet-filter')
export class UmbSelectCollectionFacetFilterElement extends UmbLitElement {
	@state()
	private _multiple = false;

	#manifest?: ManifestCollectionFacetFilter;
	public get manifest(): ManifestCollectionFacetFilter | undefined {
		return this.#manifest;
	}
	public set manifest(manifest: ManifestCollectionFacetFilter | undefined) {
		this.#manifest = manifest;
		this._multiple = !!(manifest?.meta as MetaCollectionFacetFilterSelect | undefined)?.multiple;
	}

	@state()
	private _options: Array<UmbSelectOption> = [];

	@state()
	private _value: Array<string> = [];

	@state()
	private _valueItems: Array<UmbDatalistItemModel> = [];

	@state()
	private _hasMore = false;

	#api?: UmbSelectCollectionFacetFilterApi | undefined;
	public get api(): UmbSelectCollectionFacetFilterApi | undefined {
		return this.#api;
	}
	public set api(api: UmbSelectCollectionFacetFilterApi | undefined) {
		this.#api = api;
		this.observe(api?.options, (options) => (this._options = options ?? []));
		this.observe(api?.value, (value) => (this._value = value ?? []));
		this.observe(api?.valueItems, (items) => (this._valueItems = items ?? []));

		if (api) {
			this.observe(
				observeMultiple([api.pagination.currentPage, api.pagination.totalPages]),
				([currentPage, totalPages]) => (this._hasMore = currentPage < totalPages),
			);
		}

		this.#api?.loadOptions();
	}

	#onRadioChange(event: Event) {
		const target = event.currentTarget as HTMLInputElement;
		this.#api?.setValue(target.value ? [target.value] : []);
	}

	#onCheckboxChange(event: Event) {
		const target = event.currentTarget as HTMLInputElement;
		const item = this._options.find((option) => option.value === target.value);
		if (!item) return;
		const value = target.checked ? [...this._value, item.value] : this._value.filter((v) => v !== item.value);
		this.#api?.setValue(value);
	}

	#onComboboxSelect(event: Event) {
		const target = event.target as HTMLInputElement;
		this.#api?.setValue([target.value]);
	}

	#onLoadMore() {
		this.#api?.loadMoreOptions();
	}

	#getDropdownLabel() {
		const length = this._value.length;
		const max = 2;
		if (length === 0) return this.localize.term('general_all');

		const labels = this._value
			.slice(0, max)
			.map((unique) => this._valueItems.find((i) => i.unique === unique)?.name ?? unique);

		return labels.join(', ') + (length > max ? ' + ' + (length - max) : '');
	}

	protected override render() {
		const inline = this._options.length < INLINE_THRESHOLD && !this._hasMore;
		if (inline) {
			return this._multiple ? this.#renderCheckboxList() : this.#renderRadioList();
		}
		return this._multiple ? this.#renderCheckboxDropdown() : this.#renderCombobox();
	}

	#renderRadioList() {
		return html`
			<div class="filter">
				<span class="label">${this.#manifest?.meta.label}</span>
				<uui-radio-group .value=${this._value[0] ?? ''} @change=${this.#onRadioChange}>
					${repeat(
						this._options,
						(option) => option.value,
						(option) => html`<uui-radio label=${option.label} value=${option.value}></uui-radio>`,
					)}
				</uui-radio-group>
			</div>
		`;
	}

	#renderCheckboxList() {
		return html`
			<div class="filter">
				<span class="label">${this.#manifest?.meta.label}</span>
				<div class="filter-list">
					${repeat(
						this._options,
						(option) => option.value,
						(option) => html`
							<uui-checkbox
								label=${ifDefined(option.label)}
								value=${ifDefined(option.value)}
								@change=${this.#onCheckboxChange}
								.checked=${this._value.includes(option.value)}></uui-checkbox>
						`,
					)}
				</div>
			</div>
		`;
	}

	#renderCombobox() {
		return html`
			<div class="filter">
				<span class="label">${this.#manifest?.meta?.label ?? 'Filter'}:</span>
				<uui-combobox value=${this._value[0] ?? ''} @change=${this.#onComboboxSelect} placeholder="Placeholder">
					<uui-combobox-list>
						${repeat(
							this._options,
							(option) => option.value,
							(option) => html`
								<uui-combobox-list-option value=${option.value}>${option.label}</uui-combobox-list-option>
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

	#renderCheckboxDropdown() {
		const popoverId = `umb-select-filter-${this.#manifest?.alias}`;
		return html`
			<div class="filter">
				<span class="label">${this.#manifest?.meta?.label}</span>
				<uui-button popovertarget=${popoverId} label="Select filter" compact>
					<b>${this.#getDropdownLabel()}</b>
				</uui-button>
				<uui-popover-container id=${popoverId} placement="bottom">
					<umb-popover-layout>
						<div class="filter-dropdown">
							${repeat(
								this._options,
								(option) => option.value,
								(option) => html`
									<uui-checkbox
										label=${ifDefined(option.label)}
										value=${ifDefined(option.value)}
										@change=${this.#onCheckboxChange}
										.checked=${this._value.includes(option.value)}></uui-checkbox>
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
			.label {
				font-weight: 600;
				font-size: var(--uui-size-4);
			}
			.filter-list {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
			uui-radio-group {
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
		`,
	];
}

export { UmbSelectCollectionFacetFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-select-collection-facet-filter': UmbSelectCollectionFacetFilterElement;
	}
}
