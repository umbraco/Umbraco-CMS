import { customElement, html, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-user-picker-property-editor-value-summary')
export class UmbUserPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<span class="name">${this._value[0]}</span>`;
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

export { UmbUserPickerPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-picker-property-editor-value-summary': UmbUserPickerPropertyEditorValueSummaryElement;
	}
}
