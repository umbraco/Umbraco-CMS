import type { ManifestTiptapExtension } from '../../extensions/tiptap-extension.js';
import { UmbTiptapIconRegistry } from './icon.registry.js';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

@customElement('umb-tiptap-fixed-menu')
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
		this.#editor?.on('selectionUpdate', this.#onUpdate);
		this.#editor?.on('update', this.#onUpdate);
	}
	get editor() {
		return this.#editor;
	}
	#editor?: Editor;

	#onUpdate = () => {
		this.requestUpdate();
	};

	#registry = new UmbTiptapIconRegistry();

	constructor() {
		super();
		this.#registry.attach(this);
	}

	override render() {
		return html`
			<umb-extension-with-api-slot
				type="tiptapExtension"
				.filter=${(ext: ManifestTiptapExtension) => !!ext.kind || !!ext.element}
				.elementProps=${{ editor: this.editor }}>
			</umb-extension-with-api-slot>
		`;
	}

	static override readonly styles = css`
		:host {
			border-radius: var(--uui-border-radius);
			border: 1px solid var(--uui-color-border);
			background-color: var(--uui-color-surface);
			color: var(--color-text);
			display: grid;
			grid-template-columns: repeat(auto-fill, minmax(24px, 1fr));
			position: sticky;
			top: -25px;
			left: 0px;
			right: 0px;
			padding: 4px;
		}

		:host([readonly]) {
			pointer-events: none;
			background-color: var(--uui-color-surface-alt);
		}

		button {
			color: var(--uui-color-interactive);
			width: 24px;
			height: 24px;
			padding: 4px;
			border: none;
			background: none;
			cursor: pointer;
			margin: 0;
			border-radius: 4px;
			box-sizing: border-box;
		}

		button:hover {
			color: var(--uui-color-interactive-emphasis);
			background-color: var(--uui-color-surface-alt);
		}

		button.active {
			background-color: var(--uui-color-selected);
			color: var(--uui-color-selected-contrast);
		}
		button.active:hover {
			background-color: var(--uui-color-selected-emphasis);
		}

		button img {
			width: 100%;
			height: 100%;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-fixed-menu': UmbTiptapFixedMenuElement;
	}
}
