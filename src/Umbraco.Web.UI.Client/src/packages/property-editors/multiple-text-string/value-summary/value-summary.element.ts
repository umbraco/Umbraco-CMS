import { customElement, html, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-multiple-text-string-property-editor-value-summary')
export class UmbMultipleTextStringPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		const text = this._value.join(', ');
		return html`<span class="text" title="${text}">${text}</span>`;
	}

	static override styles = css`
		.text {
			display: block;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
			max-width: 20ch;
		}
	`;
}

export { UmbMultipleTextStringPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-multiple-text-string-property-editor-value-summary': UmbMultipleTextStringPropertyEditorValueSummaryElement;
	}
}
