import type { ManifestTiptapToolbarExtensionButtonKind } from '../../extensions/index.js';
import type { UmbTiptapToolbarElementApi } from '../../extensions/types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
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
		const label = this.localize.string(this.manifest?.meta.label);
		return html`
			<uui-button
				compact
				look=${this.isActive ? 'outline' : 'default'}
				label=${label}
				title=${label}
				?disabled=${this.api?.isDisabled(this.editor)}
				@click=${() => this.api?.execute(this.editor)}>
				${when(
					this.manifest?.meta.icon,
					(icon) => html`<umb-icon name=${icon}></umb-icon>`,
					() => html`<span>${label}</span>`,
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
