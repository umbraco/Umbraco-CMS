import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-tags-property-editor-value-summary')
export class UmbTagsPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		const text = this._value.join(', ');
		return html`<span title="${text}">${text}</span>`;
	}
}

export { UmbTagsPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tags-property-editor-value-summary': UmbTagsPropertyEditorValueSummaryElement;
	}
}
