import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

@customElement('umb-tiptap-hover-menu')
export class UmbTiptapHoverMenuElement extends UmbLitElement {
	@property({ attribute: false })
	get editor() {
		return this.#editor;
	}
	set editor(value) {
		const oldValue = this.#editor;
		if (value === oldValue) {
			return;
		}
		this.#editor = value;
		this.#editor?.on('selectionUpdate', this.#onUpdate);
		this.#editor?.on('update', this.#onUpdate);
	}
	#editor?: Editor;

	override connectedCallback(): void {
		super.connectedCallback();
		this.setAttribute('popover', 'auto');
	}

	readonly #onUpdate = () => {
		if (this.editor?.isActive('table')) {
			// show the popover
			this.showPopover();
		} else {
			this.hidePopover();
		}
	};

	override render() {
		return html`<uui-popover-container></uui-popover-container>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-hover-menu': UmbTiptapHoverMenuElement;
	}
}
