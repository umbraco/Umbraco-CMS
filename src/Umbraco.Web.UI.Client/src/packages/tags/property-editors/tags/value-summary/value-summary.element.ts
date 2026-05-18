import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-tags-value-summary')
export class UmbTagsValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<uui-tag>${this._value.length}</uui-tag>`;
	}
}

export { UmbTagsValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tags-value-summary': UmbTagsValueSummaryElement;
	}
}
