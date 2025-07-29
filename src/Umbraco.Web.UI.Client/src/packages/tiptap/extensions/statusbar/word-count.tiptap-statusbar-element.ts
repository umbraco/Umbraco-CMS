import { customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tiptap-statusbar-word-count')
export class UmbTiptapStatusbarWordCountElement extends UmbLitElement {
	@state()
	private _characters = 0;

	@state()
	private _words = 0;

	@state()
	private _showCharacters = false;

	public editor?: Editor;

	override connectedCallback() {
		super.connectedCallback();

		if (this.editor) {
			this.editor.on('update', this.#onEditorUpdate);
			this.#onEditorUpdate();
		}
	}

	override disconnectedCallback() {
		super.disconnectedCallback();

		if (this.editor) {
			this.editor.off('update', this.#onEditorUpdate);
		}
	}

	readonly #onEditorUpdate = () => {
		this._characters = this.editor?.storage.characterCount.characters() ?? 0;
		this._words = this.editor?.storage.characterCount.words() ?? 0;
	};

	readonly #onClick = () => (this._showCharacters = !this._showCharacters);

	override render() {
		const label = this._showCharacters
			? this.localize.term('tiptap_statusbar_characters', this._characters)
			: this.localize.term('tiptap_statusbar_words', this._words);
		return html`<uui-button compact label=${label} @click=${this.#onClick}></uui-button>`;
	}
}

export { UmbTiptapStatusbarWordCountElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-statusbar-word-count': UmbTiptapStatusbarWordCountElement;
	}
}
