import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-checkbox-list-property-editor-value-summary')
export class UmbCheckboxListPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<uui-tag>${this._value.length}</uui-tag>`;
	}
}

export { UmbCheckboxListPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-checkbox-list-property-editor-value-summary': UmbCheckboxListPropertyEditorValueSummaryElement;
	}
}
