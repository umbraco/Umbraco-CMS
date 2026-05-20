import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbUserItemModel } from '../../../repository';

@customElement('umb-user-picker-property-editor-value-summary')
export class UmbUserPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbUserItemModel>
> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<span>${this._value[0].name}</span>`;
	}
}

export { UmbUserPickerPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-picker-property-editor-value-summary': UmbUserPickerPropertyEditorValueSummaryElement;
	}
}
