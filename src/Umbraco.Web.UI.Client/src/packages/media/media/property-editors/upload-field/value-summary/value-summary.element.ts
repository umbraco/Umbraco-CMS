import type { UmbMediaValueType } from '../types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-upload-field-property-editor-value-summary')
export class UmbUploadFieldPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<UmbMediaValueType> {
	override render() {
		if (!this._value?.src) return nothing;
		const filename = this._value.src.split('/').pop() ?? this._value.src;
		return html`<span title="${filename}">${filename}</span>`;
	}
}

export { UmbUploadFieldPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-upload-field-property-editor-value-summary': UmbUploadFieldPropertyEditorValueSummaryElement;
	}
}
