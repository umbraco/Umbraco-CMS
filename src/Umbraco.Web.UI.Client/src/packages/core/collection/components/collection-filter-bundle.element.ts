import { UMB_FACET_FILTER_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/facet-filter';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../../facet-filter/facet-filter.element.js';

@customElement('umb-collection-filter-bundle')
export class UmbCollectionFilterBundleElement extends UmbLitElement {
	#filterManager?: typeof UMB_FACET_FILTER_MANAGER_CONTEXT.TYPE;

	@state()
	private _filters: Array<any> = [];

	@state()
	private _totalActiveFilters = 0;

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

			this.observe(manager.totalActiveFilters, (total) => {
				this._totalActiveFilters = total ?? 0;
			});

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
			<uui-button compact popovertarget="collection-filter-bundle-popover" label="Filters">
				<umb-icon name="icon-equalizer"></umb-icon>
				Filters ${this._totalActiveFilters > 0 ? html`<uui-badge>${this._totalActiveFilters}</uui-badge>` : nothing}
			</uui-button>
			<uui-popover-container id="collection-filter-bundle-popover" placement="bottom-end">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${repeat(
							this._filters,
							(filter) => filter.alias,
							(filter) => html`
								<umb-facet-filter .alias=${filter.alias}>
									<div class="filter-item">
										<div class="filter-header">
											<span class="heading">${filter.manifest?.meta?.label ?? filter.alias}</span>
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
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			:host {
				display: var(--umb-collection-filter-bundle-display, contents);
			}

			.filter-dropdown {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
				padding: var(--uui-size-space-5);
				min-width: 250px;
				max-height: 70vh;
				overflow-y: auto;
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

			.heading {
				font-weight: 700;
				font-size: var(--uui-type-default-size);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-filter-bundle': UmbCollectionFilterBundleElement;
	}
}
