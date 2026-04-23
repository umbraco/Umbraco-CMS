import '../../facet-filter/facet-filter.element.js';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_FACET_FILTER_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/facet-filter';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-filter-sidebar')
export class UmbCollectionFilterSidebarElement extends UmbLitElement {
	@state()
	private _filters: Array<any> = [];

	#filterManager?: typeof UMB_FACET_FILTER_MANAGER_CONTEXT.TYPE;

	@state()
	private _activeFilterAliases: Set<string> = new Set();

	constructor() {
		super();

		new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'facetFilter',
			undefined,
			undefined,
			(filters) => {
				this._filters = filters;
			},
		);

		this.consumeContext(UMB_FACET_FILTER_MANAGER_CONTEXT, (manager) => {
			if (!manager) return;
			this.#filterManager = manager;
			this.observe(manager.activeFilters, (filters) => {
				this._activeFilterAliases = new Set(filters?.map((f) => f.alias) ?? []);
			});
		});
	}

	#isFilterActive(alias: string): boolean {
		return this._activeFilterAliases.has(alias);
	}

	#onClearFilter(alias: string) {
		this.#filterManager?.clearFilter(alias);
	}

	override render() {
		if (!this._filters.length) return nothing;

		return html`
			<div id="sidebar">
				<span class="heading">Filters</span>
				${repeat(
					this._filters,
					(filter) => filter.alias,
					(filter) => html`
						<umb-facet-filter .alias=${filter.alias}>
							<div class="filter-item">
								<div class="filter-header">
									<span class="filter-label">${filter.manifest?.meta?.label ?? filter.alias}</span>
									${this.#isFilterActive(filter.alias)
										? html`<uui-button
												look="secondary"
												label="Clear"
												@click=${() => this.#onClearFilter(filter.alias)}
												compact>
												Clear
											</uui-button>`
										: nothing}
								</div>
								${filter.component}
							</div>
						</umb-facet-filter>
					`,
				)}
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: contents;
			}

			#sidebar {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
				padding: var(--uui-size-space-5);
				border-left: 1px solid var(--uui-color-border);
				min-width: 250px;
				overflow-y: auto;
				flex-shrink: 0;
				background-color: var(--uui-color-surface);
			}

			.heading {
				font-weight: 700;
				font-size: var(--uui-type-default-size);
			}

			.filter-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
				padding-top: var(--uui-size-space-2);
			}

			.filter-header {
				display: flex;
				justify-content: space-between;
				align-items: center;
			}

			.filter-label {
				font-weight: 600;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-filter-sidebar': UmbCollectionFilterSidebarElement;
	}
}
