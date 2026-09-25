import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiStartNodeAccessElementBase } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-document-start-node-access-property-editor-ui')
export class UmbDocumentStartNodeAccessPropertyEditorUIElement extends UmbPropertyEditorUiStartNodeAccessElementBase {
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
		'umb-document-start-node-access-property-editor-ui': UmbDocumentStartNodeAccessPropertyEditorUIElement;
	}
}
