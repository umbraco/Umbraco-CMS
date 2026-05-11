import { customElement, css, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-tags-value-summary')
export class UmbTagsValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<div class="tags">${this._value.map((tag) => html`<uui-tag>${tag}</uui-tag>`)}</div>`;
	}

	static override styles = css`
		.tags {
			display: flex;
			flex-wrap: wrap;
			gap: var(--uui-size-space-1);
		}
	`;
}

export { UmbTagsValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tags-value-summary': UmbTagsValueSummaryElement;
	}
}
