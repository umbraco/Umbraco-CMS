import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import { UMB_FACET_FILTER_MANAGER_CONTEXT, type UmbActiveFacetFilterModel } from '@umbraco-cms/backoffice/facet-filter';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-active-filters')
export class UmbCollectionActiveFiltersElement extends UmbLitElement {
	#filterManager?: typeof UMB_FACET_FILTER_MANAGER_CONTEXT.TYPE;

	@state()
	private _activeFilters?: Array<UmbActiveFacetFilterModel> = [];

	@state()
	private _totalItems: number = 0;

	constructor() {
		super();

		this.consumeContext(UMB_FACET_FILTER_MANAGER_CONTEXT, (manager) => {
			this.#filterManager = manager;

			this.observe(manager?.activeFilters, (activeFilters) => {
				this._activeFilters = activeFilters;
			});
		});

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.observe(context?.totalItems, (total) => {
				this._totalItems = total ?? 0;
			});
		});
	}

	override render() {
		const hasActiveFilters = this._activeFilters && this._activeFilters.length > 0;

		if (!hasActiveFilters) {
			return html`
				<div id="active-filters">
					<small>Showing <strong>${this._totalItems}</strong> ${this._totalItems === 1 ? 'result' : 'results'}</small>
				</div>
			`;
		}

		return html`
			<div id="active-filters">
				<small>Showing results for:</small>
				<div id="active-filter-items">
					${repeat(
						this._activeFilters!,
						(activeFilter) => `${activeFilter.alias}||${activeFilter.unique}`,
						(activeFilter) => this.#renderActiveFilterItem(activeFilter),
					)}
				</div>
			</div>
		`;
	}

	#renderActiveFilterItem(activeFilter: UmbActiveFacetFilterModel) {
		return html`
			<span class="active-filter-item">
				${activeFilter.unique}
				<uui-button
					label="Clear"
					compact
					@click=${() => this.#onClearFilterValue(activeFilter.alias, activeFilter.unique)}>
					<umb-icon name="icon-delete"></umb-icon>
				</uui-button>
			</span>
		`;
	}

	#onClearFilterValue(alias: string, unique: string) {
		this.#filterManager?.clearFilterValue(alias, unique);
	}

	static override styles = [
		UmbTextStyles,
		css`
			#active-filters {
				display: block;
				padding: var(--uui-size-space-3) 0;
			}

			#active-filter-items {
				display: inline;
			}

			.active-filter-item {
				display: inline-flex;
				align-items: center;
				padding: 0 0 0 var(--uui-size-space-3);
				font-size: 12px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-active-filters': UmbCollectionActiveFiltersElement;
	}
}
