import { customElement, html, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbMemberItemModel } from '../../../types';

@customElement('umb-member-picker-property-editor-value-summary')
export class UmbMemberPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbMemberItemModel>
> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<span class="name">${this._value[0].name}</span>`;
	}

	static override styles = css`
		.name {
			display: block;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
			max-width: 20ch;
		}
	`;
}

export { UmbMemberPickerPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-picker-property-editor-value-summary': UmbMemberPickerPropertyEditorValueSummaryElement;
	}
}
