import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbMemberGroupItemModel } from '../../../types.js';

@customElement('umb-member-group-picker-property-editor-value-summary')
export class UmbMemberGroupPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbMemberGroupItemModel>
> {
	override render() {
		if (!Array.isArray(this._value) || !this._value.length) return nothing;
		const text = this._value.map((item) => item.name).join(', ');
		return html`<span title="${text}">${text}</span>`;
	}
}

export { UmbMemberGroupPickerPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-picker-property-editor-value-summary': UmbMemberGroupPickerPropertyEditorValueSummaryElement;
	}
}
