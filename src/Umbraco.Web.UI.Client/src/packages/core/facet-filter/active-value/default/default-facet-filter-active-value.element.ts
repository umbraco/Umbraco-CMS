import { UmbFacetFilterActiveValueElementBase } from '../base/facet-filter-active-value-element.base.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

/**
 * Default facet filter active value element. Shows the `name` of the resolved item, or the raw value when there is none.
 */
@customElement('umb-default-facet-filter-active-value')
export class UmbDefaultFacetFilterActiveValueElement extends UmbFacetFilterActiveValueElementBase {
	#getName(): string {
		const resolvedValue = this._resolvedValue;
		if (
			resolvedValue &&
			typeof resolvedValue === 'object' &&
			'name' in resolvedValue &&
			typeof resolvedValue.name === 'string'
		) {
			return resolvedValue.name;
		}
		return String(this.api?.value ?? '');
	}

	override render() {
		return html`<umb-facet-filter-active-value-layout @remove=${this._remove}>
			${this.#getName()}
		</umb-facet-filter-active-value-layout>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-facet-filter-active-value': UmbDefaultFacetFilterActiveValueElement;
	}
}
