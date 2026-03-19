import '../filter/facet-filter/collection-facet-filter-item.element.js';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-filter-sidebar')
export class UmbCollectionFilterSidebarElement extends UmbLitElement {
	@state()
	private _filters: Array<any> = [];

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
						<umb-collection-facet-filter-item .filterAlias=${filter.alias}>
							${filter.component}
						</umb-collection-facet-filter-item>
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
		'umb-collection-filter-sidebar': UmbCollectionFilterSidebarElement;
	}
}
