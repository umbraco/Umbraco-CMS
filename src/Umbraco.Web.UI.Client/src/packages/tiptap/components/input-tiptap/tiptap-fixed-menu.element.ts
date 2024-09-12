import {
	alignCenter,
	alignJustify,
	alignLeft,
	alignRight,
	blockquote,
	bold,
	bulletList,
	code,
	heading1,
	heading2,
	heading3,
	horizontalRule,
	italic,
	link,
	orderedList,
	strikethrough,
	underline,
} from './icons.js';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { LitElement, css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

@customElement('umb-tiptap-fixed-menu')
export class UmbTiptapFixedMenuElement extends LitElement {
	@state()
	actions = [
		// TODO: I don't think we need a paragraph button. It's the default state.
		// {
		// 	name: 'paragraph',
		// 	icon: html`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="20" height="20">
		// 		<path fill="none" d="M0 0h24v24H0z" />
		// 		<path d="M12 6v15h-2v-5a6 6 0 1 1 0-12h10v2h-3v15h-2V6h-3zm-2 0a4 4 0 1 0 0 8V6z" fill="currentColor" />
		// 	</svg>`,
		// 	command: () => this.editor?.chain().focus().setParagraph().run(),
		// },
		{
			name: 'bold',
			icon: bold,
			isActive: () => this.editor?.isActive('bold'),
			command: () => this.editor?.chain().focus().toggleBold().run(),
		},
		{
			name: 'italic',
			icon: italic,
			isActive: () => this.editor?.isActive('italic'),
			command: () => this.editor?.chain().focus().toggleItalic().run(),
		},
		{
			name: 'underline',
			icon: underline,
			isActive: () => this.editor?.isActive('underline'),
			command: () => this.editor?.chain().focus().toggleUnderline().run(),
		},
		{
			name: 'strikethrough',
			icon: strikethrough,
			isActive: () => this.editor?.isActive('strike'),
			command: () => this.editor?.chain().focus().toggleStrike().run(),
		},
		{
			name: 'h1',
			icon: heading1,
			isActive: () => this.editor?.isActive('heading', { level: 1 }),
			command: () => this.editor?.chain().focus().toggleHeading({ level: 1 }).run(),
		},
		{
			name: 'h2',
			icon: heading2,
			isActive: () => this.editor?.isActive('heading', { level: 2 }),
			command: () => this.editor?.chain().focus().toggleHeading({ level: 2 }).run(),
		},
		{
			name: 'h3',
			icon: heading3,
			isActive: () => this.editor?.isActive('heading', { level: 3 }),
			command: () => this.editor?.chain().focus().toggleHeading({ level: 3 }).run(),
		},
		{
			name: 'blockquote',
			icon: blockquote,
			isActive: () => this.editor?.isActive('blockquote'),
			command: () => this.editor?.chain().focus().toggleBlockquote().run(),
		},
		{
			name: 'code',
			icon: code,
			isActive: () => this.editor?.isActive('codeBlock'),
			command: () => this.editor?.chain().focus().toggleCodeBlock().run(),
		},
		{
			name: 'bullet-list',
			icon: bulletList,
			isActive: () => this.editor?.isActive('bulletList'),
			command: () => this.editor?.chain().focus().toggleBulletList().run(),
		},
		{
			name: 'ordered-list',
			icon: orderedList,
			isActive: () => this.editor?.isActive('orderedList'),
			command: () => this.editor?.chain().focus().toggleOrderedList().run(),
		},
		{
			name: 'horizontal-rule',
			icon: horizontalRule,
			isActive: () => this.editor?.isActive('horizontalRule'),
			command: () => this.editor?.chain().focus().setHorizontalRule().run(),
		},
		{
			name: 'align-left',
			icon: alignLeft,
			isActive: () => this.editor?.isActive({ textAlign: 'left' }),
			command: () => this.editor?.chain().focus().setTextAlign('left').run(),
		},
		{
			name: 'align-center',
			icon: alignCenter,
			isActive: () => this.editor?.isActive({ textAlign: 'center' }),
			command: () => this.editor?.chain().focus().setTextAlign('center').run(),
		},
		{
			name: 'align-right',
			icon: alignRight,
			isActive: () => this.editor?.isActive({ textAlign: 'right' }),
			command: () => this.editor?.chain().focus().setTextAlign('right').run(),
		},
		{
			name: 'align-justify',
			icon: alignJustify,
			isActive: () => this.editor?.isActive({ textAlign: 'justify' }),
			command: () => this.editor?.chain().focus().setTextAlign('justify').run(),
		},
		{
			name: 'link',
			icon: link,
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

	#onUpdate = () => {
		this.requestUpdate();
	};

	override render() {
		return html`
			${this.actions.map(
				(action) => html`
					<button class=${action.isActive?.() ? 'active' : ''} @click=${action.command} title=${action.name}>
						${action.icon}
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
