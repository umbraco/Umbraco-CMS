import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-code-editor-value-summary')
export class UmbCodeEditorValueSummaryElement extends UmbValueSummaryElementBase<string | undefined> {
	override render() {
		if (!this._value?.trim()) return nothing;
		return html`<uui-icon name="icon-document-html"></uui-icon>`;
	}
}

export { UmbCodeEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-editor-value-summary': UmbCodeEditorValueSummaryElement;
	}
}
