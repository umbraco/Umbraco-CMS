import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-markdown-editor-property-editor-value-summary')
export class UmbMarkdownEditorPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<string | undefined> {
	override render() {
		if (!this._value?.trim()) return nothing;
		return html`<uui-icon name="icon-document-html"></uui-icon>`;
	}
}

export { UmbMarkdownEditorPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-markdown-editor-property-editor-value-summary': UmbMarkdownEditorPropertyEditorValueSummaryElement;
	}
}
