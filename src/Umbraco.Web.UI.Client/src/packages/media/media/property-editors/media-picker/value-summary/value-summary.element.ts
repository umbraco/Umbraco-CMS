import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbMediaItemModel } from '../../../types.js';

@customElement('umb-media-picker-property-editor-value-summary')
export class UmbMediaPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbMediaItemModel>
> {
	override render() {
		if (!this._value?.length) return nothing;
		const names = this._value.map((item) => item.name).filter(Boolean);
		return html`<span title="${names.join(', ')}">${names.join(', ')}</span>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-picker-property-editor-value-summary': UmbMediaPickerPropertyEditorValueSummaryElement;
	}
}
