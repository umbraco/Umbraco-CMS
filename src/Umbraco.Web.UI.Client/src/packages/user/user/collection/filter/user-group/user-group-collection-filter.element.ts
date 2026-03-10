import type { UmbUserGroupCollectionFilterApi } from './user-group-collection-filter.api.js';
import { css, customElement, html, ifDefined, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import type { ManifestCollectionFilter, UmbSelectOption } from '@umbraco-cms/backoffice/collection';

@customElement('umb-user-group-collection-filter')
export class UmbUserGroupCollectionFilterElement extends UmbLitElement {
	public manifest?: ManifestCollectionFilter;

	@state()
	private _options: Array<UmbSelectOption> = [];

	@state()
	private _value: Array<string> = [];

	@state()
	private _valueLabels: Map<string, string> = new Map();

	@state()
	private _hasMore = false;

	private _api?: UmbUserGroupCollectionFilterApi | undefined;
	public get api(): UmbUserGroupCollectionFilterApi | undefined {
		return this._api;
	}
	public set api(api: UmbUserGroupCollectionFilterApi | undefined) {
		this._api = api;
		this.observe(api?.options, (options) => (this._options = options ?? []));
		this.observe(api?.value, (value) => {
			this._value = value ?? [];
			this.#resolveValueLabels();
		});

		if (api) {
			this.observe(
				observeMultiple([api.pagination.currentPage, api.pagination.totalPages]),
				([currentPage, totalPages]) => (this._hasMore = currentPage < totalPages),
			);
		}

		this._api?.loadOptions();
	}

	async #resolveValueLabels() {
		if (!this._api || this._value.length === 0) {
			this._valueLabels = new Map();
			return;
		}

		const unresolvedUniques = this._value.filter((unique) => !this._valueLabels.has(unique));
		if (unresolvedUniques.length === 0) return;

		const items = await this._api.requestItems(unresolvedUniques);
		const updatedLabels = new Map(this._valueLabels);
		for (const item of items) {
			updatedLabels.set(item.unique, item.name ?? item.unique);
		}
		this._valueLabels = updatedLabels;
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
		const labels = this._value.slice(0, max).map((unique) => this._valueLabels.get(unique) ?? unique);
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
										@change=${this.#onChange}></uui-checkbox>
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

export { UmbUserGroupCollectionFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection-filter': UmbUserGroupCollectionFilterElement;
	}
}
