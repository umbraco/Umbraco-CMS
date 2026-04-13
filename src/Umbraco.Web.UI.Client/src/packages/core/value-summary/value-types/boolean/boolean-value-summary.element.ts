import { UmbValueSummaryElementBase } from '../../components/value-summary-element.base.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-boolean-value-summary')
export class UmbBooleanValueSummaryElement extends UmbValueSummaryElementBase<boolean> {
	override render() {
		return html`${this._value === true
			? html`<uui-icon name="icon-true"></uui-icon>`
			: html`<uui-icon name="icon-false"></uui-icon>`}`;
	}
}

export { UmbBooleanValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-boolean-value-summary': UmbBooleanValueSummaryElement;
	}
}
