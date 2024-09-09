import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './tiptap-fixed-menu.element.js';
import { Editor, StarterKit } from '@umbraco-cms/backoffice/external/tiptap';

@customElement('umb-input-tiptap')
export class UmbInputTiptapElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	@state()
	_editor!: Editor;

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		const editor = this.shadowRoot?.querySelector('#editor');

		if (!editor) return;

		const json =
			this.value && typeof this.value === 'string'
				? JSON.parse(this.value)
				: {
						type: 'doc',
						content: [
							{
								type: 'paragraph',
								content: [
									{
										type: 'text',
										text: 'Hello Umbraco',
									},
								],
							},
						],
					};

		// TODO: Try Disable css inject to remove prosemirror css
		this._editor = new Editor({
			element: editor,
			extensions: [StarterKit],
			content: json,
			onUpdate: ({ editor }) => {
				const json = editor.getJSON();
				this.value = JSON.stringify(json);
				console.log('json', json);
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
			#editor {
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-border);
				margin: 0 auto;
				box-sizing: border-box;
				height: 100%;
				width: 100%;
				padding: 1rem;
				overflow: clip;
				min-height: 400px;
				display: grid; /* Don't ask me why this is needed, but it is. */
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
			}
			.ProseMirror p,
			.ProseMirror h1,
			.ProseMirror h2,
			.ProseMirror h3 {
				margin-top: 0;
				margin-bottom: 0.1rem;
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
