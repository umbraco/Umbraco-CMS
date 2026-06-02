import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-code-editor-property-editor-value-summary')
export class UmbCodeEditorPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<string | undefined> {
	override render() {
		if (!this._value?.trim()) return nothing;
		return html`<uui-icon name="icon-check"></uui-icon>`;
	}
}

export { UmbCodeEditorPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-editor-property-editor-value-summary': UmbCodeEditorPropertyEditorValueSummaryElement;
	}
}
