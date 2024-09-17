import * as icons from './icons.js';
import type { UmbTiptapExtensionBase, UmbTiptapToolbarButton } from './tiptap-extension.js';
import { css, customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

@customElement('umb-tiptap-fixed-menu')
export class UmbTiptapFixedMenuElement extends UmbLitElement {
	@state()
	actions: Array<UmbTiptapToolbarButton> = [
		// TODO: I don't think we need a paragraph button. It's the default state.
		// {
		// 	name: 'paragraph',
		// 	icon: html`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="20" height="20">
		// 		<path fill="none" d="M0 0h24v24H0z" />
		// 		<path d="M12 6v15h-2v-5a6 6 0 1 1 0-12h10v2h-3v15h-2V6h-3zm-2 0a4 4 0 1 0 0 8V6z" fill="currentColor" />
		// 	</svg>`,
		// 	command: (editor) => editor?.chain().focus().setParagraph().run(),
		// },
		{
			name: 'bold',
			icon: icons.bold,
			isActive: (editor) => editor?.isActive('bold'),
			command: (editor) => editor?.chain().focus().toggleBold().run(),
		},
		{
			name: 'italic',
			icon: icons.italic,
			isActive: (editor) => editor?.isActive('italic'),
			command: (editor) => editor?.chain().focus().toggleItalic().run(),
		},
		{
			name: 'underline',
			icon: icons.underline,
			isActive: (editor) => editor?.isActive('underline'),
			command: (editor) => editor?.chain().focus().toggleUnderline().run(),
		},
		{
			name: 'strikethrough',
			icon: icons.strikethrough,
			isActive: (editor) => editor?.isActive('strike'),
			command: (editor) => editor?.chain().focus().toggleStrike().run(),
		},
		{
			name: 'h1',
			icon: icons.heading1,
			isActive: (editor) => editor?.isActive('heading', { level: 1 }),
			command: (editor) => editor?.chain().focus().toggleHeading({ level: 1 }).run(),
		},
		{
			name: 'h2',
			icon: icons.heading2,
			isActive: (editor) => editor?.isActive('heading', { level: 2 }),
			command: (editor) => editor?.chain().focus().toggleHeading({ level: 2 }).run(),
		},
		{
			name: 'h3',
			icon: icons.heading3,
			isActive: (editor) => editor?.isActive('heading', { level: 3 }),
			command: (editor) => editor?.chain().focus().toggleHeading({ level: 3 }).run(),
		},
		{
			name: 'blockquote',
			icon: icons.blockquote,
			isActive: (editor) => editor?.isActive('blockquote'),
			command: (editor) => editor?.chain().focus().toggleBlockquote().run(),
		},
		{
			name: 'code',
			icon: icons.code,
			isActive: (editor) => editor?.isActive('codeBlock'),
			command: (editor) => editor?.chain().focus().toggleCodeBlock().run(),
		},
		{
			name: 'bullet-list',
			icon: icons.bulletList,
			isActive: (editor) => editor?.isActive('bulletList'),
			command: (editor) => editor?.chain().focus().toggleBulletList().run(),
		},
		{
			name: 'ordered-list',
			icon: icons.orderedList,
			isActive: (editor) => editor?.isActive('orderedList'),
			command: (editor) => editor?.chain().focus().toggleOrderedList().run(),
		},
		{
			name: 'horizontal-rule',
			icon: icons.horizontalRule,
			isActive: (editor) => editor?.isActive('horizontalRule'),
			command: (editor) => editor?.chain().focus().setHorizontalRule().run(),
		},
		{
			name: 'align-left',
			icon: icons.alignLeft,
			isActive: (editor) => editor?.isActive({ textAlign: 'left' }),
			command: (editor) => editor?.chain().focus().setTextAlign('left').run(),
		},
		{
			name: 'align-center',
			icon: icons.alignCenter,
			isActive: (editor) => editor?.isActive({ textAlign: 'center' }),
			command: (editor) => editor?.chain().focus().setTextAlign('center').run(),
		},
		{
			name: 'align-right',
			icon: icons.alignRight,
			isActive: (editor) => editor?.isActive({ textAlign: 'right' }),
			command: (editor) => editor?.chain().focus().setTextAlign('right').run(),
		},
		{
			name: 'align-justify',
			icon: icons.alignJustify,
			isActive: (editor) => editor?.isActive({ textAlign: 'justify' }),
			command: (editor) => editor?.chain().focus().setTextAlign('justify').run(),
		},
		{
			name: 'link',
			icon: icons.link,
			isActive: (editor) => editor?.isActive('link'),
			command: () => {
				const text = prompt('Enter the text');
				const url = prompt('Enter the URL');

				if (url && text && this.editor) {
					const { from } = this.editor.state.selection;
					this.editor
						.chain()
						.focus()
						.insertContent(text)
						.setTextSelection({ from: from, to: from + text.length })
						.setLink({ href: url, target: '_blank' })
						.run();
				}
			},
		},
	];

	@property({ attribute: false })
	set editor(value) {
		const oldValue = this.#editor;
		if (value === oldValue) {
			return;
		}
		this.#editor = value;
		this.#editor?.on('selectionUpdate', this.#onUpdate);
		this.#editor?.on('update', this.#onUpdate);
		// todo add listener for commands
	}
	get editor() {
		return this.#editor;
	}
	#editor?: Editor;

	@property({ attribute: false })
	extensions: Array<UmbTiptapExtensionBase> = [];

	#onUpdate = () => {
		this.requestUpdate();
	};

	protected override firstUpdated() {
		const buttons = this.extensions.flatMap((ext) => ext.getToolbarButtons());
		this.actions.push(...buttons);
	}

	override render() {
		return html`
			${this.actions.map(
				(action) => html`
					<button
						class=${action.isActive?.(this.editor) ? 'active' : ''}
						title=${action.name}
						@click=${() => action.command(this.editor)}>
						${when(
							typeof action.icon === 'string',
							() => html`<umb-icon name=${action.icon}></umb-icon>`,
							() => action.icon,
						)}
					</button>
				`,
			)}
		`;
	}

	static override styles = css`
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
