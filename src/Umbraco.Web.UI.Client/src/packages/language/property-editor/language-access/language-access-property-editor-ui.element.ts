import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbStartNodeAccessPropertyEditorUiElementBase } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-language-access')
export class UmbLanguageAccessPropertyEditorUIElement extends UmbStartNodeAccessPropertyEditorUiElementBase {
	#onChange(event: CustomEvent & { target: { selection: Array<string> } }) {
		this._onPick(event.target.selection);
	}

	protected override renderPicker() {
		return html`
			<umb-input-language
				.selection=${this._selection}
				.min=${this._min}
				.max=${this._max}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-language>
		`;
	}
}

export { UmbLanguageAccessPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-language-access': UmbLanguageAccessPropertyEditorUIElement;
	}
}
