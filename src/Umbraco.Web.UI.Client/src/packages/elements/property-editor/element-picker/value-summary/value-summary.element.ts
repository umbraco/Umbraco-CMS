import { customElement, html, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-element-picker-value-summary')
export class UmbElementPickerValueSummaryElement extends UmbValueSummaryElementBase<Array<string> | undefined> {
	override render() {
		if (!Array.isArray(this._value) || !this._value.length) return nothing;
		return html`<span>${this._value.length}</span><umb-icon name="icon-plugin"></umb-icon>`;
	}

	static override styles = css`
		:host {
			display: flex;
			align-items: center;
			gap: 4px;
		}
	`;
}

export { UmbElementPickerValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-picker-value-summary': UmbElementPickerValueSummaryElement;
	}
}
