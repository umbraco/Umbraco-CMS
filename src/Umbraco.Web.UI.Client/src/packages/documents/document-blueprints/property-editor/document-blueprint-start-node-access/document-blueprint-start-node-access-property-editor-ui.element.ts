import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbStartNodeAccessPropertyEditorUiElementBase } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-document-blueprint-start-node-access')
export class UmbDocumentBlueprintStartNodeAccessPropertyEditorUIElement extends UmbStartNodeAccessPropertyEditorUiElementBase {
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

export { UmbDocumentBlueprintStartNodeAccessPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-blueprint-start-node-access': UmbDocumentBlueprintStartNodeAccessPropertyEditorUIElement;
	}
}
