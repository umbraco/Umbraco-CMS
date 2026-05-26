import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbPropertyEditorRteValueType } from '@umbraco-cms/backoffice/rte';

@customElement('umb-tiptap-property-editor-value-summary')
export class UmbTiptapPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	UmbPropertyEditorRteValueType | undefined
> {
	override render() {
		const temp = document.createElement('div');
		temp.innerHTML = this._value?.markup ?? '';
		if (!temp.textContent?.trim()) return nothing;
		return html`<uui-icon name="icon-check"></uui-icon>`;
	}
}

export { UmbTiptapPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-property-editor-value-summary': UmbTiptapPropertyEditorValueSummaryElement;
	}
}
