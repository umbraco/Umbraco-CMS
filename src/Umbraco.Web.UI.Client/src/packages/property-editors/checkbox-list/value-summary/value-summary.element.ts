import { customElement, html, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-checkbox-list-property-editor-value-summary')
export class UmbCheckboxListPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		const text = this._value.join(', ');
		return html`<span title="${text}">${text}</span>`;
	}
}

export { UmbCheckboxListPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-checkbox-list-property-editor-value-summary': UmbCheckboxListPropertyEditorValueSummaryElement;
	}
}
