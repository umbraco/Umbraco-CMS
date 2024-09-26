import type { ManifestTiptapExtensionButtonKind } from '../../extensions/tiptap-extension.js';
import type { UmbTiptapToolbarElementApi } from '../../extensions/types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { customElement, html, ifDefined, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-tiptap-toolbar-button';

@customElement(elementName)
export class UmbTiptapToolbarButtonElement extends UmbLitElement {
	public api?: UmbTiptapToolbarElementApi;
	public editor?: Editor;
	public manifest?: ManifestTiptapExtensionButtonKind;

	@state()
	private _isActive = false;

	override connectedCallback() {
		super.connectedCallback();

		if (this.editor) {
			this.editor.on('selectionUpdate', this.#onEditorUpdate);
			this.editor.on('update', this.#onEditorUpdate);
		}
	}

	override disconnectedCallback() {
		super.disconnectedCallback();

		if (this.editor) {
			this.editor.off('selectionUpdate', this.#onEditorUpdate);
			this.editor.off('update', this.#onEditorUpdate);
		}
	}

	readonly #onEditorUpdate = () => {
		if (this.api && this.editor && this.manifest) {
			this._isActive = this.api.isActive(this.editor);
		}
	};

	override render() {
		return html`
			<uui-button
				compact
				look=${this._isActive ? 'outline' : 'default'}
				label=${ifDefined(this.manifest?.meta.label)}
				title=${this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : ''}
				@click=${() => (this.api && this.editor ? this.api.execute(this.editor) : null)}>
				${when(
					this.manifest?.meta.icon,
					() => html`<umb-icon name=${this.manifest!.meta.icon}></umb-icon>`,
					() => html`<span>${this.manifest?.meta.label}</span>`,
				)}
			</uui-button>
		`;
	}
}

export { UmbTiptapToolbarButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbTiptapToolbarButtonElement;
	}
}
