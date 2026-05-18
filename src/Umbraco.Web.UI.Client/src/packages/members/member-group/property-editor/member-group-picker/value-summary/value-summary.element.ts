import { customElement, html, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-member-group-picker-value-summary')
export class UmbMemberGroupPickerValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!Array.isArray(this._value) || !this._value.length) return nothing;
		return html`<span class="name" title="${this._value.join(', ')}">${this._value.join(', ')}</span>`;
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

export { UmbMemberGroupPickerValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-picker-value-summary': UmbMemberGroupPickerValueSummaryElement;
	}
}
