import type { ManifestFacetFilterActiveValue } from '../facet-filter-active-value.extension.js';
import { UmbDefaultFacetFilterActiveValueApi } from '../default/default-facet-filter-active-value.api.js';
import { UmbDefaultFacetFilterActiveValueElement } from '../default/default-facet-filter-active-value.element.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Renders one facet filter active value with the `facetFilterActiveValue` extension registered for its facet filter, or with the
 * `default` kind's element and API when none is.
 * @element umb-facet-filter-active-value-extension
 */
@customElement('umb-facet-filter-active-value-extension')
export class UmbFacetFilterActiveValueExtensionElement extends UmbLitElement {
	#alias?: string;
	#filter: (manifest: ManifestFacetFilterActiveValue) => boolean = () => false;
	#fallbackElement?: UmbDefaultFacetFilterActiveValueElement;
	#fallbackApi?: UmbDefaultFacetFilterActiveValueApi;

	/**
	 * The alias of the facet filter the value belongs to.
	 */
	@property({ attribute: false })
	set alias(alias: string | undefined) {
		if (this.#alias === alias) return;
		this.#alias = alias;
		this.#filter = (manifest) => !!alias && manifest.forFacetFilters.includes(alias);
	}
	get alias() {
		return this.#alias;
	}

	/**
	 * The unique of the active filter entry.
	 */
	@property({ attribute: false })
	unique?: string;

	/**
	 * The raw active value.
	 */
	@property({ attribute: false })
	value?: unknown;

	#renderFallback = () => {
		if (!this.#fallbackElement || !this.#fallbackApi) {
			this.#fallbackElement = new UmbDefaultFacetFilterActiveValueElement();
			this.#fallbackApi = new UmbDefaultFacetFilterActiveValueApi(this.#fallbackElement);
			this.#fallbackElement.api = this.#fallbackApi;
		}
		this.#fallbackApi.alias = this.#alias;
		this.#fallbackApi.unique = this.unique;
		this.#fallbackApi.value = this.value;
		return this.#fallbackElement;
	};

	override render() {
		if (!this.#alias || this.unique === undefined) return nothing;
		return html`<umb-extension-with-api-slot
			type="facetFilterActiveValue"
			single
			.filter=${this.#filter}
			.apiProps=${{ alias: this.#alias, unique: this.unique, value: this.value }}
			.fallbackRenderMethod=${this.#renderFallback}></umb-extension-with-api-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-facet-filter-active-value-extension': UmbFacetFilterActiveValueExtensionElement;
	}
}
