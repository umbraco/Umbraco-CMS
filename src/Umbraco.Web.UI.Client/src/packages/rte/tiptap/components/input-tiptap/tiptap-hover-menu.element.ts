import { LitElement, css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

@customElement('umb-tiptap-hover-menu')
export class UmbTiptapHoverMenuElement extends LitElement {
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
		this.setAttribute('popover', '');
	}

	readonly #onUpdate = () => {
		if (this.editor?.isActive('link')) {
			// show the popover
			this.showPopover();
		} else {
			this.requestUpdate();
		}
	};

	override render() {
		return html`<uui-popover-container></uui-popover-container>`;
	}

	static override readonly styles = css`
		:host {
			position: fixed;
			background-color: var(--uui-color-surface-alt);
			border: 1px solid var(--uui-color-border);
			border-radius: var(--uui-size-border-radius);
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-hover-menu': UmbTiptapHoverMenuElement;
	}
}
