import type { ManifestFacetFilter } from '../facet-filter.extension.js';
import type { MetaFacetFilterSelect } from './types.js';
import type { UmbSelectFacetFilterApi } from './select-facet-filter.api.js';
import type { UmbDatalistItemModel, UmbDatalistOptionModel } from '@umbraco-cms/backoffice/datalist-data-source';
import { css, customElement, html, ifDefined, repeat, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

type UmbSelectValue = Pick<UmbDatalistOptionModel, 'unique' | 'entityType'>;

const INLINE_THRESHOLD = 6;

@customElement('umb-select-facet-filter')
export class UmbSelectFacetFilterElement extends UmbLitElement {
	@state()
	private _multiple = false;

	#manifest?: ManifestFacetFilter;
	public get manifest(): ManifestFacetFilter | undefined {
		return this.#manifest;
	}
	public set manifest(manifest: ManifestFacetFilter | undefined) {
		this.#manifest = manifest;
		this._multiple = !!(manifest?.meta as MetaFacetFilterSelect | undefined)?.multiple;
	}

	@state()
	private _options: Array<UmbDatalistOptionModel> = [];

	@state()
	private _value: Array<UmbSelectValue> = [];

	@state()
	private _valueItems: Array<UmbDatalistItemModel> = [];

	@state()
	private _hasMore = false;

	@state()
	private _comboboxSearch: string = '';

	#api?: UmbSelectFacetFilterApi | undefined;
	public get api(): UmbSelectFacetFilterApi | undefined {
		return this.#api;
	}
	public set api(api: UmbSelectFacetFilterApi | undefined) {
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
		if (!target.value) {
			this.#api?.setValue([]);
			return;
		}
		const option = this._options.find((o) => o.unique === target.value);
		if (option) this.#api?.setValue([{ unique: option.unique, entityType: option.entityType }]);
	}

	#onCheckboxChange(event: Event) {
		const target = event.currentTarget as HTMLInputElement;
		const item = this._options.find((option) => option.unique === target.value);
		if (!item) return;
		const newValue = target.checked
			? [...this._value, { unique: item.unique, entityType: item.entityType }]
			: this._value.filter((v) => v.unique !== item.unique);
		this.#api?.setValue(newValue);
	}

	#onComboboxSelect(event: Event) {
		const target = event.target as HTMLInputElement;
		if (!target.value) {
			this.#api?.setValue([]);
			return;
		}
		const option = this._options.find((o) => o.unique === target.value);
		if (option) this.#api?.setValue([{ unique: option.unique, entityType: option.entityType }]);
	}

	#onComboboxSearch(event: Event) {
		const target = event.target as any;
		this._comboboxSearch = target.search ?? '';
	}

	get #filteredOptions() {
		if (!this._comboboxSearch) return this._options;
		const text = this._comboboxSearch.toLowerCase();
		return this._options.filter((option) => option.name?.toLowerCase().includes(text));
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
			.map((item) => this._valueItems.find((i) => i.unique === item.unique)?.name ?? item.unique);

		return labels.join(', ') + (length > max ? ' + ' + (length - max) : '');
	}

	#onDropdownSearch(event: Event) {
		const target = event.target as HTMLInputElement;
		this._comboboxSearch = target.value ?? '';
	}

	#formatOptionLabel(option: UmbDatalistOptionModel): string {
		const name = option.name ?? option.unique;
		return option.count !== undefined ? `${name} (${option.count})` : name;
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
				<uui-radio-group .value=${this._value[0]?.unique ?? ''} @change=${this.#onRadioChange}>
					${repeat(
						this._options,
						(option) => option.unique,
						(option) => html`<uui-radio label=${this.#formatOptionLabel(option)} value=${option.unique}></uui-radio>`,
					)}
				</uui-radio-group>
			</div>
		`;
	}

	#renderCheckboxList() {
		return html`
			<div class="filter">
				<div class="filter-list">
					${repeat(
						this._options,
						(option) => option.unique,
						(option) => html`
							<uui-checkbox
								label=${this.#formatOptionLabel(option)}
								value=${ifDefined(option.unique)}
								@change=${this.#onCheckboxChange}
								.checked=${this._value.some((v) => v.unique === option.unique)}></uui-checkbox>
						`,
					)}
				</div>
			</div>
		`;
	}

	#renderCombobox() {
		return html`
			<div class="filter">
				<uui-combobox
					value=${this._value[0]?.unique ?? ''}
					@change=${this.#onComboboxSelect}
					@search=${this.#onComboboxSearch}
					placeholder="Placeholder">
					<uui-combobox-list>
						${repeat(
							this.#filteredOptions,
							(option) => option.unique,
							(option) => html`
								<uui-combobox-list-option value=${option.unique}
									>${this.#formatOptionLabel(option)}</uui-combobox-list-option
								>
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
				<button class="dropdown-trigger" popovertarget=${popoverId}>
					<span class="dropdown-trigger-label">${this.#getDropdownLabel()}</span>
					<uui-symbol-expand .open=${false}></uui-symbol-expand>
				</button>
				<uui-popover-container id=${popoverId} placement="bottom">
					<umb-popover-layout>
						<div class="filter-dropdown">
							<uui-input
								type="search"
								placeholder=${this.localize.term('general_search')}
								label=${this.localize.term('general_search')}
								@input=${this.#onDropdownSearch}
								.value=${this._comboboxSearch}>
							</uui-input>
							${repeat(
								this.#filteredOptions,
								(option) => option.unique,
								(option) => html`
									<uui-checkbox
										label=${this.#formatOptionLabel(option)}
										value=${ifDefined(option.unique)}
										@change=${this.#onCheckboxChange}
										.checked=${this._value.some((v) => v.unique === option.unique)}></uui-checkbox>
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
				min-width: 200px;
			}

			.dropdown-trigger {
				display: flex;
				align-items: center;
				justify-content: space-between;
				padding: var(--uui-size-space-2) var(--uui-size-space-3);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				background: var(--uui-color-surface);
				cursor: pointer;
				font-size: var(--uui-size-4);
				min-height: 36px;
			}

			.dropdown-trigger:hover {
				border-color: var(--uui-color-border-emphasis);
			}

			.dropdown-trigger-label {
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}
		`,
	];
}

export { UmbSelectFacetFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-select-facet-filter': UmbSelectFacetFilterElement;
	}
}
