import { UmbFilterActiveValueElementBase } from '../base/filter-active-value-element.base.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

/**
 * Default filter active value element. Shows the `name` of the resolved item, or the raw value when there is none.
 */
@customElement('umb-default-filter-active-value')
export class UmbDefaultFilterActiveValueElement extends UmbFilterActiveValueElementBase {
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
		return html`<umb-filter-active-value-layout
			.label=${this.#getName()}
			@remove=${this._remove}></umb-filter-active-value-layout>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-filter-active-value': UmbDefaultFilterActiveValueElement;
	}
}
