import type { ManifestTiptapToolbarExtensionActionButtonKind } from '../../extensions/tiptap-toolbar.extension.js';
import { UmbTiptapToolbarButtonElement } from './tiptap-toolbar-button.element.js';
import { customElement, html, when } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-tiptap-toolbar-button-action')
export class UmbTiptapToolbarButtonActionElement extends UmbTiptapToolbarButtonElement<ManifestTiptapToolbarExtensionActionButtonKind> {
	// Note: This element does not use the inherited `isActive` @state in its template,
	// but relies on it being set by the base class `#onEditorUpdate` to trigger Lit re-renders
	// so that `api.isDisabled(editor)` is re-evaluated.
	override render() {
		const label = this.localize.string(this.manifest?.meta.label);
		return html`
			<uui-button
				compact
				look="default"
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

export { UmbTiptapToolbarButtonActionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-button-action': UmbTiptapToolbarButtonActionElement;
	}
}
