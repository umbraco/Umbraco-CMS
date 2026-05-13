import { customElement, html, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-member-picker-value-summary')
export class UmbMemberPickerValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<span class="name" title="${this._value[0]}">${this._value[0]}</span>`;
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

export { UmbMemberPickerValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-picker-value-summary': UmbMemberPickerValueSummaryElement;
	}
}
