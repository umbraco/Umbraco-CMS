import { EXAMPLE_COLLECTION_ALIAS } from '../collection/constants.js';
import { html, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';

@customElement('example-dashboard-with-collection')
export class ExampleDashboardWithCollection extends UmbElementMixin(LitElement) {
	#config: UmbCollectionConfiguration = {
		pageSize: 3,
	};

	override render() {
		return html`<umb-collection alias=${EXAMPLE_COLLECTION_ALIAS} .config=${this.#config}></umb-collection>`;
	}
}

export { ExampleDashboardWithCollection as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-dashboard-with-collection': ExampleDashboardWithCollection;
	}
}
