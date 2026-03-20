import { UmbFacetFilterContext } from './facet-filter.context.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-facet-filter')
export class UmbFacetFilterElement extends UmbLitElement {
	#context = new UmbFacetFilterContext(this);

	#alias?: string;
	public get alias(): string | undefined {
		return this.#alias;
	}
	public set alias(alias: string | undefined) {
		this.#alias = alias;
		if (alias) {
			this.#context.setAlias(alias);
		}
	}

	override render() {
		return html`<slot></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-facet-filter': UmbFacetFilterElement;
	}
}
