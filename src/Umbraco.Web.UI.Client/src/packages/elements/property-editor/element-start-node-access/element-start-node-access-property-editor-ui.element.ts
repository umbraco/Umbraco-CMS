import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiStartNodeAccessElementBase } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-element-start-node-access-property-editor-ui')
export class UmbElementStartNodeAccessPropertyEditorUIElement extends UmbPropertyEditorUiStartNodeAccessElementBase {
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

export default UmbElementStartNodeAccessPropertyEditorUIElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-start-node-access-property-editor-ui': UmbElementStartNodeAccessPropertyEditorUIElement;
	}
}
