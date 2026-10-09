import { UmbMediaPickerFolderFilter } from '../../components/input-media/input-media.context.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbStartNodeAccessPropertyEditorUiElementBase } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-media-start-node-access')
export class UmbMediaStartNodeAccessPropertyEditorUIElement extends UmbStartNodeAccessPropertyEditorUiElementBase {
	#onChange(event: CustomEvent & { target: { selection: Array<string> } }) {
		this._onPick(event.target.selection);
	}

	protected override renderPicker() {
		return html`
			<umb-input-media
				.selection=${this._selection}
				.min=${this._min}
				.max=${this._max}
				.folderFilter=${UmbMediaPickerFolderFilter.FOLDERS_ONLY}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-media>
		`;
	}
}

export { UmbMediaStartNodeAccessPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-start-node-access': UmbMediaStartNodeAccessPropertyEditorUIElement;
	}
}
