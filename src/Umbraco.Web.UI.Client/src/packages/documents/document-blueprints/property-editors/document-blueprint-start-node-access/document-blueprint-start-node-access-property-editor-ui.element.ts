import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiStartNodeAccessElementBase } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-document-blueprint-start-node-access-property-editor-ui')
export class UmbDocumentBlueprintStartNodeAccessPropertyEditorUIElement extends UmbPropertyEditorUiStartNodeAccessElementBase {
	#onChange(event: CustomEvent & { target: { selection: Array<string> } }) {
		this._onPick(event.target.selection);
	}

	protected override renderPicker() {
		return html`
			<umb-input-document-blueprint
				.selection=${this._selection}
				.min=${this._min}
				.max=${this._max}
				?folderOnly=${true}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-document-blueprint>
		`;
	}
}

export default UmbDocumentBlueprintStartNodeAccessPropertyEditorUIElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-start-node-access-property-editor-ui': UmbDocumentBlueprintStartNodeAccessPropertyEditorUIElement;
	}
}
