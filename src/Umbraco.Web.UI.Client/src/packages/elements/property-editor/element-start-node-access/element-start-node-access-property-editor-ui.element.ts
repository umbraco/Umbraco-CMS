import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbStartNodeAccessPropertyEditorUiElementBase } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-element-start-node-access')
export class UmbElementStartNodeAccessPropertyEditorUIElement extends UmbStartNodeAccessPropertyEditorUiElementBase {
	#onChange(event: CustomEvent & { target: { selection: Array<string> } }) {
		this._onPick(event.target.selection);
	}

	protected override renderPicker() {
		return html`
			<umb-input-element
				.selection=${this._selection}
				.min=${this._min}
				.max=${this._max}
				?folderOnly=${true}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-element>
		`;
	}
}

export { UmbElementStartNodeAccessPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-element-start-node-access': UmbElementStartNodeAccessPropertyEditorUIElement;
	}
}
