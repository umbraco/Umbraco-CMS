import { UMB_FACET_FILTER_MANAGER_CONTEXT, type UmbActiveFacetFilterModel } from '@umbraco-cms/backoffice/facet-filter';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-active-filters')
export class UmbCollectionActiveFiltersElement extends UmbLitElement {
	#filterManager?: typeof UMB_FACET_FILTER_MANAGER_CONTEXT.TYPE;

	@state()
	private _activeFilters?: Array<UmbActiveFacetFilterModel> = [];

	constructor() {
		super();

		this.consumeContext(UMB_FACET_FILTER_MANAGER_CONTEXT, (manager) => {
			this.#filterManager = manager;

			this.observe(manager?.activeFilters, (activeFilters) => {
				this._activeFilters = activeFilters;
			});
		});
	}

	override render() {
		const hasActiveFilters = this._activeFilters && this._activeFilters.length > 0;

		if (!hasActiveFilters) {
			return nothing;
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
				${String(activeFilter.value)}
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
		this.#filterManager?.clearActiveFilter(alias, unique);
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: var(--umb-collection-active-filters-display, contents);
			}

			#active-filters {
				display: block;
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
