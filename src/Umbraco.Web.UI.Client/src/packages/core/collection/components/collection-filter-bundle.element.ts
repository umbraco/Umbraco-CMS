import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-filter-bundle')
export class UmbCollectionFilterBundleElement extends UmbLitElement {
	@state()
	private _filters: Array<any> = [];

	@state()
	private _totalActiveFilters = 0;

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

			this.observe(context.filtering.totalActiveFilters, (total) => {
				this._totalActiveFilters = total ?? 0;
			});
		});
	}

	override render() {
		if (!this._filters.length) return nothing;

		return html`
			<uui-button compact popovertarget="collection-filter-bundle-popover" label="Filters" look="outline">
				<umb-icon name="icon-equalizer"></umb-icon>
				Filters ${this._totalActiveFilters > 0 ? html`<uui-badge>${this._totalActiveFilters}</uui-badge>` : nothing}
			</uui-button>
			<uui-popover-container id="collection-filter-bundle-popover" placement="bottom-end">
				<umb-popover-layout>
					<div class="filter-dropdown">
						<span class="heading">Filters:</span>
						${repeat(
							this._filters,
							(filter) => filter.alias,
							(filter) => filter.component,
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
