import type { UmbMemberItemModel } from '../../../types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

/** Renders the picked member names, comma-joined, for collection view cells. */
@customElement('umb-multiple-member-picker-property-editor-value-summary')
export class UmbMultipleMemberPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbMemberItemModel>
> {
	override render() {
		if (!this._value?.length) return nothing;
		const text = this._value
			.map((item) => item.name)
			.filter(Boolean)
			.join(', ');
		if (!text) return nothing;
		return html`<span title="${text}">${text}</span>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-multiple-member-picker-property-editor-value-summary': UmbMultipleMemberPickerPropertyEditorValueSummaryElement;
	}
}
