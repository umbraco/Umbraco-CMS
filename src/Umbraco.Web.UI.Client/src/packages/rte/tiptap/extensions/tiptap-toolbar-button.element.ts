import type { ManifestTiptapExtensionButtonKind } from './tiptap-extension.js';
import type { UmbTiptapExtensionApi } from './types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { customElement, html, ifDefined, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-tiptap-toolbar-button';

@customElement(elementName)
export class UmbTiptapToolbarButtonElement extends UmbLitElement {
	public api?: UmbTiptapExtensionApi;
	public editor?: Editor;
	public manifest?: ManifestTiptapExtensionButtonKind;

	override render() {
		return html`
			<uui-button compact label=${ifDefined(this.manifest?.meta.label)} @click=${() => this.api?.execute(this.editor)}>
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
