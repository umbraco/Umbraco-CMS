import { UmbCollectionFacetFilterContext } from './collection-facet-filter.context.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-facet-filter-item')
export class UmbCollectionFacetFilterItemElement extends UmbLitElement {
	#context = new UmbCollectionFacetFilterContext(this);

	#filterAlias?: string;
	public get filterAlias(): string | undefined {
		return this.#filterAlias;
	}
	public set filterAlias(alias: string | undefined) {
		this.#filterAlias = alias;
		if (alias) {
			this.#context.setFilterAlias(alias);
		}
	}

	override render() {
		return html`<slot></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-facet-filter-item': UmbCollectionFacetFilterItemElement;
	}
}
