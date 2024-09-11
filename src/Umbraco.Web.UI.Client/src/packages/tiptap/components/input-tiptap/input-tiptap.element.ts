import type { UmbTiptapFixedMenuElement } from './tiptap-fixed-menu.element.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, customElement, html, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './tiptap-fixed-menu.element.js';
import { Editor, Link, StarterKit, TextAlign, Underline } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-tiptap')
export class UmbInputTiptapElement extends UUIFormControlMixin(UmbLitElement, '') {
	@query('umb-tiptap-fixed-menu') _fixedMenuElement!: UmbTiptapFixedMenuElement;
	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	@state()
	_editor!: Editor;

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		const editor = this.shadowRoot?.querySelector('#editor');

		if (!editor) return;

		const json = this.value && typeof this.value === 'string' ? JSON.parse(this.value) : this.value;

		// TODO: Try Disable css inject to remove prosemirror css
		this._editor = new Editor({
			element: editor,
			extensions: [
				StarterKit,
				TextAlign.configure({
					types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
				}),
				Link,
				Underline,
			],
			content: json,
			onSelectionUpdate: ({ editor }) => {
				const { $from } = editor.state.selection;
				const activeMarks = $from.node();

				// Log the active marks
				console.log('Active Marks:', activeMarks);
				this._fixedMenuElement.onUpdate(); // TODO: This is a hack to force the fixed menu to update. We need to find a better way.
			},
			onUpdate: ({ editor }) => {
				const json = editor.getJSON();
				this.value = JSON.stringify(json);
				this.dispatchEvent(new UmbChangeEvent());
				this._fixedMenuElement.onUpdate(); // TODO: This is a hack to force the fixed menu to update. We need to find a better way.
			},
		});
	}

	protected getFormElement() {
		return null;
	}

	override render() {
		return html`
			<umb-tiptap-fixed-menu
				class="uui-text uui-font"
				.activeNodeOrMark=${this._editor?.isActive('bold') ? 'bold' : null}
				.editor=${this._editor}></umb-tiptap-fixed-menu>
			<div id="editor"></div>
		`;
	}

	static override styles = [
		css`
			#editor {
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-border);
				border-top-left-radius: 0;
				border-top-right-radius: 0;
				border-top: 0;
				margin: 0 auto;
				box-sizing: border-box;
				height: 100%;
				width: 100%;
				padding: 1rem;
				overflow: clip;
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
			.ProseMirror {
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
