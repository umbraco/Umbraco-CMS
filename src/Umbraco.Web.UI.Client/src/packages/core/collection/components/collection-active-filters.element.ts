import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { UmbCollectionActiveFilterModel } from '../filter/collection-filter.manager.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-collection-active-filters')
export class UmbCollectionActiveFiltersElement extends UmbLitElement {
	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	@state()
	private _activeFilters?: Array<UmbCollectionActiveFilterModel> = [];

	#filterLabelsMap = new Map<string, string>();

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;

			this.observe(this.#collectionContext?.filtering.activeFilters, (activeFilters) => {
				this._activeFilters = activeFilters;
				if (!activeFilters || activeFilters.length === 0) return;

				this.observe(
					umbExtensionsRegistry.byTypeAndAliases(
						'collectionFilter',
						activeFilters.map((x) => x.alias),
					),
					(extensions) => {
						extensions.forEach((extension) => {
							const alias = extension.alias;
							const label = extension.meta?.label ?? alias;
							this.#filterLabelsMap.set(alias, label);
							this.requestUpdate();
						});
					},
				);
			});
		});
	}

	override render() {
		if (!this._activeFilters || this._activeFilters.length === 0) return nothing;

		return html`
			<div id="active-filters">
				<small>Showing results for:</small>
				<div id="active-filter-items">
					${repeat(
						this._activeFilters,
						(activeFilter) => activeFilter.alias,
						(activeFilter) => this.#renderActiveFilterItem(activeFilter),
					)}
				</div>
				<uui-button compact look="secondary" label="Clear all" @click=${this.#onClearAllFilters}>
					Clear all
				</uui-button>
			</div>
		`;
	}

	#renderActiveFilterItem(activeFilter: UmbCollectionActiveFilterModel) {
		const label = this.#filterLabelsMap.get(activeFilter.alias) ?? activeFilter.alias;
		const value = Array.isArray(activeFilter.value) ? activeFilter.value.join(', ') : activeFilter.value;
		return html` <span class="active-filter-item"> <strong>${label}:</strong> ${value} </span> `;
	}

	#onClearAllFilters() {
		if (!this._activeFilters) return;
		for (const filter of this._activeFilters) {
			this.#collectionContext?.filtering.removeFilter(filter.alias);
		}
		this.#collectionContext?.loadCollection();
	}

	static override styles = [
		UmbTextStyles,
		css`
			#active-filters {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-3) 0;
				flex-wrap: wrap;
			}

			#active-filters small {
				color: var(--uui-color-text-alt);
			}

			#active-filter-items {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}

			.active-filter-item {
				font-size: var(--uui-type-small-size);
			}

			.active-filter-item:not(:last-child)::after {
				content: '|';
				margin-left: var(--uui-size-space-2);
				color: var(--uui-color-border);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-active-filters': UmbCollectionActiveFiltersElement;
	}
}
