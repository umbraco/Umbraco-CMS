import type { ManifestTiptapToolbarExtensionButtonKind } from '../../extensions/index.js';
import type { UmbTiptapToolbarElementApi } from '../../extensions/types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { customElement, html, ifDefined, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tiptap-toolbar-button')
export class UmbTiptapToolbarButtonElement extends UmbLitElement {
	public api?: UmbTiptapToolbarElementApi;
	public editor?: Editor;
	public manifest?: ManifestTiptapToolbarExtensionButtonKind;

	@state()
	protected isActive = false;

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
			this.isActive = this.api.isActive(this.editor);
		}
	};

	override render() {
		return html`
			<uui-button
				compact
				look=${this.isActive ? 'outline' : 'default'}
				label=${ifDefined(this.manifest?.meta.label)}
				title=${this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : ''}
				?disabled=${this.api && this.editor && this.api.isDisabled(this.editor)}
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
		'umb-tiptap-toolbar-button': UmbTiptapToolbarButtonElement;
	}
}
