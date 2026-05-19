import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-toggle-property-editor-value-summary')
export class UmbTogglePropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<boolean> {
	override render() {
		if (this._value === undefined) return nothing;
		return html`${this._value === true
			? html`<uui-icon name="icon-true"></uui-icon>`
			: html`<uui-icon name="icon-false"></uui-icon>`}`;
	}
}

export { UmbTogglePropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-toggle-property-editor-value-summary': UmbTogglePropertyEditorValueSummaryElement;
	}
}
