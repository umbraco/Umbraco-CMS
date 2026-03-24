import { DYNAMIC_FACET_COLLECTION_ALIAS } from '../collection/constants.js';
import { html, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';

@customElement('example-dynamic-facet-filters-dashboard')
export class ExampleDynamicFacetFiltersDashboard extends UmbElementMixin(LitElement) {
	#config: UmbCollectionConfiguration = {
		pageSize: 50,
	};

	override render() {
		return html`<umb-collection alias=${DYNAMIC_FACET_COLLECTION_ALIAS} .config=${this.#config}></umb-collection>`;
	}
}

export { ExampleDynamicFacetFiltersDashboard as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-dynamic-facet-filters-dashboard': ExampleDynamicFacetFiltersDashboard;
	}
}
