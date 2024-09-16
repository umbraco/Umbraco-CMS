import type { UmbTiptapFixedMenuElement } from './tiptap-fixed-menu.element.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, customElement, html, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './tiptap-fixed-menu.element.js';
import './tiptap-hover-menu.element.js';
import { Editor, Link, StarterKit, TextAlign, Underline } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-tiptap')
export class UmbInputTiptapElement extends UUIFormControlMixin(UmbLitElement, '') {
	@query('umb-tiptap-fixed-menu') _fixedMenuElement!: UmbTiptapFixedMenuElement;
	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	@state()
	_editor!: Editor;

	protected override firstUpdated(): void {
		const editor = this.shadowRoot?.querySelector('#editor');

		if (!editor) return;

		const toolbar = this.configuration?.getValueByAlias<string[]>('toolbar');
		const maxWidth = this.configuration?.getValueByAlias<number>('maxWidth');
		const maxHeight = this.configuration?.getValueByAlias<number>('maxHeight');
		const mode = this.configuration?.getValueByAlias<string>('mode');

		this.setAttribute('style', `max-width: ${maxWidth}px;`);
		editor.setAttribute('style', `max-height: ${maxHeight}px;`);

		if (!editor) return;

		this._editor = new Editor({
			element: editor,
			extensions: [
				StarterKit,
				TextAlign.configure({
					types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
				}),
				Link.configure({ openOnClick: false }),
				Underline,
			],
			content: this.value.toString(),
			onUpdate: ({ editor }) => {
				this.value = editor.getHTML();
				this.dispatchEvent(new UmbChangeEvent());
			},
		});
	}

	protected getFormElement() {
		return null;
	}

	override render() {
		return html`
			<umb-tiptap-fixed-menu .editor=${this._editor}></umb-tiptap-fixed-menu>
			<div id="editor"></div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
			}
			#editor {
				overflow: auto;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-border);
				padding: 1rem;
				border-top-left-radius: 0;
				border-top-right-radius: 0;
				border-top: 0;
				box-sizing: border-box;
				height: 100%;
				width: 100%;
				min-height: 400px;
				display: grid; /* Don't ask me why this is needed, but it is. */
			}

			#editor pre {
				background-color: var(--uui-color-surface-alt);
				padding: var(--uui-size-space-2) var(--uui-size-space-4);
				border-radius: calc(var(--uui-border-radius) * 2);
				overflow-x: auto;
			}

			#editor code:not(pre > code) {
				background-color: var(--uui-color-surface-alt);
				padding: var(--uui-size-space-1) var(--uui-size-space-2);
				border-radius: calc(var(--uui-border-radius) * 2);
			}

			#editor code {
				font-family: 'Roboto Mono', monospace;
				background: none;
				color: inherit;
				font-size: 0.8rem;
				padding: 0;
			}
			.tiptap {
				height: 100%;
				width: 100%;
				outline: none;
				white-space: pre-wrap;
				min-width: 0;
			}
			#editor p,
			#editor h1,
			#editor h2,
			#editor h3 {
				margin-top: 0;
				margin-bottom: 0.5em;
			}
		`,
	];
}

export default UmbInputTiptapElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tiptap': UmbInputTiptapElement;
	}
}
