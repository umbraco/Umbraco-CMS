import { UMB_FACET_FILTER_MANAGER_CONTEXT, type UmbActiveFacetFilterModel } from '@umbraco-cms/backoffice/facet-filter';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-active-filters')
export class UmbCollectionActiveFiltersElement extends UmbLitElement {
	@state()
	private _activeFilters?: Array<UmbActiveFacetFilterModel> = [];

	constructor() {
		super();

		this.consumeContext(UMB_FACET_FILTER_MANAGER_CONTEXT, (manager) => {
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
						(activeFilter) =>
							html`<umb-facet-filter-active-value-extension
								.alias=${activeFilter.alias}
								.unique=${activeFilter.unique}
								.value=${activeFilter.value}></umb-facet-filter-active-value-extension>`,
					)}
				</div>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			#active-filters {
				display: block;
			}

			#active-filter-items {
				display: inline;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-active-filters': UmbCollectionActiveFiltersElement;
	}
}
