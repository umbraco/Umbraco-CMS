import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbStartNodeAccessPropertyEditorUiElementBase } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-document-start-node-access')
export class UmbDocumentStartNodeAccessPropertyEditorUIElement extends UmbStartNodeAccessPropertyEditorUiElementBase {
	#onChange(event: CustomEvent & { target: { selection: Array<string> } }) {
		this._onPick(event.target.selection);
	}

	protected override renderPicker() {
		return html`
			<umb-input-document
				.selection=${this._selection}
				.min=${this._min}
				.max=${this._max}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-document>
		`;
	}
}

export { UmbDocumentStartNodeAccessPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-start-node-access': UmbDocumentStartNodeAccessPropertyEditorUIElement;
	}
}
