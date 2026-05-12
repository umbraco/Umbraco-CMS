import type { UmbMediaValueType } from '../types.js';
import { css, customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-upload-field-value-summary')
export class UmbUploadFieldValueSummaryElement extends UmbValueSummaryElementBase<UmbMediaValueType> {
	override render() {
		if (!this._value?.src) return nothing;
		const filename = this._value.src.split('/').pop() ?? this._value.src;
		return html`<span class="filename" title="${filename}">${filename}</span>`;
	}

	static override styles = css`
		.filename {
			display: block;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
			max-width: 20ch;
		}
	`;
}

export { UmbUploadFieldValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-upload-field-value-summary': UmbUploadFieldValueSummaryElement;
	}
}
