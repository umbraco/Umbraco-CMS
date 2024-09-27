import type { ManifestTiptapToolbarExtension } from '../../extensions/tiptap-toolbar-extension.js';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

import '../toolbar/tiptap-toolbar-dropdown-base.element.js';

const elementName = 'umb-tiptap-fixed-menu';

@customElement(elementName)
export class UmbTiptapFixedMenuElement extends UmbLitElement {
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ attribute: false })
	set editor(value) {
		const oldValue = this.#editor;
		if (value === oldValue) {
			return;
		}
		this.#editor = value;
	}
	get editor() {
		return this.#editor;
	}
	#editor?: Editor;

	override render() {
		return html`
			<umb-extension-with-api-slot
				type="tiptapToolbarExtension"
				.filter=${(ext: ManifestTiptapToolbarExtension) => !!ext.kind || !!ext.element}
				.elementProps=${{ editor: this.editor }}>
			</umb-extension-with-api-slot>
		`;
	}

	static override readonly styles = css`
		:host {
			border-radius: var(--uui-border-radius);
			border: 1px solid var(--uui-color-border);
			border-bottom-left-radius: 0;
			border-bottom-right-radius: 0;
			background-color: var(--uui-color-surface);
			color: var(--color-text);
			display: grid;
			grid-template-columns: repeat(auto-fill, minmax(24px, 1fr));
			gap: var(--uui-size-space-1);
			position: sticky;
			top: -25px;
			left: 0px;
			right: 0px;
			padding: var(--uui-size-space-3);
			align-items: center;
			z-index: 100;
		}

		:host([readonly]) {
			pointer-events: none;
			background-color: var(--uui-color-surface-alt);
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbTiptapFixedMenuElement;
	}
}
