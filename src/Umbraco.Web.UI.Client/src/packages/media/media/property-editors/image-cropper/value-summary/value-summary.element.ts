import { customElement, css, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbImageCropperPropertyEditorValue } from '../../../components/index.js';

@customElement('umb-image-cropper-value-summary')
export class UmbImageCropperValueSummaryElement extends UmbValueSummaryElementBase<UmbImageCropperPropertyEditorValue> {
	override render() {
		if (!this._value?.src) return nothing;
		const filename = this._value.src.split('/').pop() ?? this._value;
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

export { UmbImageCropperValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-value-summary': UmbImageCropperValueSummaryElement;
	}
}
