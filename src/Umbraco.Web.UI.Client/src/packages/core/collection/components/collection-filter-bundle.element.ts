import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../filter/facet-filter/collection-facet-filter.element.js';

@customElement('umb-collection-filter-bundle')
export class UmbCollectionFilterBundleElement extends UmbLitElement {
	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

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
			'collectionFacetFilter',
			undefined,
			undefined,
			(filters) => {
				this._filters = filters;
			},
		);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			if (!context) return;
			this.#collectionContext = context;

			this.observe(context.filtering.totalActiveFilters, (total) => {
				this._totalActiveFilters = total ?? 0;
			});

			this.observe(context.filtering.activeFilters, (filters) => {
				this._activeFilterAliases = new Set(filters?.map((f) => f.alias) ?? []);
			});
		});
	}

	#isFilterActive(alias: string): boolean {
		return this._activeFilterAliases.has(alias);
	}

	#onClearFilter(alias: string) {
		this.#collectionContext?.filtering.clearFilter(alias);
		this.#collectionContext?.loadCollection();
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
								<umb-collection-facet-filter .alias=${filter.alias}>
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
								</umb-collection-facet-filter>
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
				display: contents;
			}

			.filter-dropdown {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
				padding: var(--uui-size-space-5);
				min-width: 250px;
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
