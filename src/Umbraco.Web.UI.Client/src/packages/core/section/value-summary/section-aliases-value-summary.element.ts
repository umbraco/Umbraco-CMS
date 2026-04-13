import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-section-aliases-value-summary')
export class UmbSectionAliasesValueSummaryElement extends UmbValueSummaryElementBase<ReadonlyArray<string>> {
	override render() {
		return html`${this._value?.join(', ')}`;
	}
}

export { UmbSectionAliasesValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-aliases-value-summary': UmbSectionAliasesValueSummaryElement;
	}
}
