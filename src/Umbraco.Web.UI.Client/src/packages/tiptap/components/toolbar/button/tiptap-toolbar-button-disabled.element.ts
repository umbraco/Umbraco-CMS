import { UmbTiptapToolbarButtonElement } from './tiptap-toolbar-button.element.js';
import { customElement, html, when } from '@umbraco-cms/backoffice/external/lit';

/** @deprecated No longer used internally. Please use `UmbTiptapToolbarButtonActionElement` (`umb-tiptap-toolbar-button-action`) instead. This will be removed in Umbraco 19. [LK] */
@customElement('umb-tiptap-toolbar-button-disabled')
export class UmbTiptapToolbarButtonDisabledElement extends UmbTiptapToolbarButtonElement {
	override render() {
		const label = this.localize.string(this.manifest?.meta.label);
		return html`
			<uui-button
				compact
				look="default"
				label=${label}
				title=${label}
				?disabled=${!this.isActive}
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

export { UmbTiptapToolbarButtonDisabledElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-button-disabled': UmbTiptapToolbarButtonDisabledElement;
	}
}
