import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbPropertyEditorRteValueType } from '@umbraco-cms/backoffice/rte';

@customElement('umb-tiptap-value-summary')
export class UmbTiptapValueSummaryElement extends UmbValueSummaryElementBase<
	UmbPropertyEditorRteValueType | undefined
> {
	override render() {
		const text = this._value?.markup?.replace(/<[^>]*>/g, '').trim();
		if (!text) return nothing;
		return html`<uui-icon name="icon-document-html"></uui-icon>`;
	}
}

export { UmbTiptapValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-value-summary': UmbTiptapValueSummaryElement;
	}
}
