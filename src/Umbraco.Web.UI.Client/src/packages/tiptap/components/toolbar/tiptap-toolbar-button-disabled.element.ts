import { UmbTiptapToolbarButtonElement } from './tiptap-toolbar-button.element.js';
import { customElement, html, ifDefined, when } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-tiptap-toolbar-button-disabled')
export class UmbTiptapToolbarButtonDisabledElement extends UmbTiptapToolbarButtonElement {
	override render() {
		return html`
			<uui-button
				compact
				look="default"
				label=${ifDefined(this.manifest?.meta.label)}
				title=${this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : ''}
				?disabled=${!this.isActive}
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

export { UmbTiptapToolbarButtonDisabledElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-button-disabled': UmbTiptapToolbarButtonDisabledElement;
	}
}
