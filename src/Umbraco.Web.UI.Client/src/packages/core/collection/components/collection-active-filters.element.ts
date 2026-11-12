import { UMB_FILTER_MANAGER_CONTEXT, type UmbActiveFilterModel } from '@umbraco-cms/backoffice/filter';
import { css, html, customElement, nothing, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-active-filters')
export class UmbCollectionActiveFiltersElement extends UmbLitElement {
	@state()
	private _activeFilters?: Array<UmbActiveFilterModel> = [];

	constructor() {
		super();

		this.consumeContext(UMB_FILTER_MANAGER_CONTEXT, (manager) => {
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
				${repeat(
					this._activeFilters!,
					(activeFilter) => `${activeFilter.alias}||${activeFilter.unique}`,
					(activeFilter) =>
						html`<umb-filter-active-value-extension
							.alias=${activeFilter.alias}
							.unique=${activeFilter.unique}
							.value=${activeFilter.value}></umb-filter-active-value-extension>`,
				)}
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: contents;
			}

			#active-filters {
				display: flex;
				flex-wrap: wrap;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-active-filters': UmbCollectionActiveFiltersElement;
	}
}
