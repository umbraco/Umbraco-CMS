import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-boolean-value-summary')
export class UmbBooleanValueSummaryElement extends UmbLitElement {
	@property({ attribute: false })
	value?: boolean | null;

	override render() {
		return html`${this.value === true
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
