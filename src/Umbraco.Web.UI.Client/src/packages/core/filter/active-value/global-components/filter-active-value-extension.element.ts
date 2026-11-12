import type { ManifestFilterActiveValue } from '../filter-active-value.extension.js';
import { UmbDefaultFilterActiveValueApi } from '../default/default-filter-active-value.api.js';
import { UmbDefaultFilterActiveValueElement } from '../default/default-filter-active-value.element.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Renders one filter active value with the `filterActiveValue` extension registered for its filter, or with the
 * `default` kind's element and API when none is.
 * @element umb-filter-active-value-extension
 */
@customElement('umb-filter-active-value-extension')
export class UmbFilterActiveValueExtensionElement extends UmbLitElement {
	#alias?: string;
	#filter: (manifest: ManifestFilterActiveValue) => boolean = () => false;
	#fallbackElement?: UmbDefaultFilterActiveValueElement;
	#fallbackApi?: UmbDefaultFilterActiveValueApi;

	/**
	 * The alias of the filter the value belongs to.
	 */
	@property({ attribute: false })
	set alias(alias: string | undefined) {
		if (this.#alias === alias) return;
		this.#alias = alias;
		this.#filter = (manifest) => !!alias && manifest.forFilters.includes(alias);
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
			this.#fallbackElement = new UmbDefaultFilterActiveValueElement();
			this.#fallbackApi = new UmbDefaultFilterActiveValueApi(this.#fallbackElement);
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
			type="filterActiveValue"
			single
			.filter=${this.#filter}
			.apiProps=${{ alias: this.#alias, unique: this.unique, value: this.value }}
			.fallbackRenderMethod=${this.#renderFallback}></umb-extension-with-api-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-filter-active-value-extension': UmbFilterActiveValueExtensionElement;
	}
}
