import { UmbTiptapToolbarDropdownBaseElement } from '../../components/index.js';
import type { UmbTiptapToolbarDropdownItem } from '../../components/index.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';

const elementName = 'umb-tiptap-style-select-toolbar-element';

@customElement(elementName)
export class UmbTiptapToolbarStyleSelectToolbarElement extends UmbTiptapToolbarDropdownBaseElement {
	public editor?: Editor;

	protected override alias = 'style-select';

	protected override label = 'Style select';

	@state()
	protected override get items(): Array<UmbTiptapToolbarDropdownItem> {
		return [
			{
				alias: 'headers',
				label: 'Headers',
				items: [
					{
						alias: 'h2',
						label: 'Page heading',
						execute: () => this.editor?.chain().focus().toggleHeading({ level: 2 }).run(),
					},
					{
						alias: 'h3',
						label: 'Section heading',
						execute: () => this.editor?.chain().focus().toggleHeading({ level: 3 }).run(),
					},
					{
						alias: 'h4',
						label: 'Paragraph heading',
						execute: () => this.editor?.chain().focus().toggleHeading({ level: 4 }).run(),
					},
				],
			},
			{
				alias: 'blocks',
				label: 'Blocks',
				items: [
					{
						alias: 'p',
						label: 'Paragraph',
						execute: () => this.editor?.chain().focus().setParagraph().run(),
					},
				],
			},
			{
				alias: 'containers',
				label: 'Containers',
				items: [
					{ alias: 'blockquote', label: 'Quote', execute: () => this.editor?.chain().focus().toggleBlockquote().run() },
					{ alias: 'code', label: 'Code', execute: () => this.editor?.chain().focus().toggleCodeBlock().run() },
				],
			},
		];
	}

	static override readonly styles = UmbTiptapToolbarDropdownBaseElement.styles;
}

export { UmbTiptapToolbarStyleSelectToolbarElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbTiptapToolbarStyleSelectToolbarElement;
	}
}
