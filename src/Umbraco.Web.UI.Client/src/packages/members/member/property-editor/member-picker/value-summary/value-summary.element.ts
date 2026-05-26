import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbMemberItemModel } from '../../../types.js';

@customElement('umb-member-picker-property-editor-value-summary')
export class UmbMemberPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbMemberItemModel>
> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<span>${this._value[0].name}</span>`;
	}
}

export { UmbMemberPickerPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-picker-property-editor-value-summary': UmbMemberPickerPropertyEditorValueSummaryElement;
	}
}
