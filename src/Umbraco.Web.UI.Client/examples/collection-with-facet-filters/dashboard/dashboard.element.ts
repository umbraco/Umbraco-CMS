import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../product/collection/constants.js';
import { html, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';

@customElement('example-facet-filters-dashboard')
export class ExampleFacetFiltersDashboard extends UmbElementMixin(LitElement) {
	#config: UmbCollectionConfiguration = {
		pageSize: 50,
	};

	override render() {
		return html`<umb-collection alias=${EXAMPLE_PRODUCT_COLLECTION_ALIAS} .config=${this.#config}></umb-collection>`;
	}
}

export { ExampleFacetFiltersDashboard as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-facet-filters-dashboard': ExampleFacetFiltersDashboard;
	}
}
