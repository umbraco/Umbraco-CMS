import type { ManifestTiptapToolbarExtension } from '../tiptap-toolbar.extension.js';
import type { UmbCascadingMenuItem } from '../../components/cascading-menu-popover/cascading-menu-popover.element.js';
import { css, customElement, html, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

import '../../components/cascading-menu-popover/cascading-menu-popover.element.js';

@customElement('umb-tiptap-style-select-toolbar-element')
export class UmbTiptapToolbarStyleSelectToolbarElement extends UmbLitElement {
	#menu: Array<UmbCascadingMenuItem> = [
		{
			unique: 'headers',
			label: 'Headers',
			items: [
				{
					unique: 'h2',
					label: 'Page heading',
					execute: () => this.editor?.chain().focus().toggleHeading({ level: 2 }).run(),
				},
				{
					unique: 'h3',
					label: 'Section heading',
					execute: () => this.editor?.chain().focus().toggleHeading({ level: 3 }).run(),
				},
				{
					unique: 'h4',
					label: 'Paragraph heading',
					execute: () => this.editor?.chain().focus().toggleHeading({ level: 4 }).run(),
				},
			],
		},
		{
			unique: 'blocks',
			label: 'Blocks',
			items: [
				{
					unique: 'p',
					label: 'Paragraph',
					execute: () => this.editor?.chain().focus().setParagraph().run(),
				},
			],
		},
		{
			unique: 'containers',
			label: 'Containers',
			items: [
				{ unique: 'blockquote', label: 'Quote', execute: () => this.editor?.chain().focus().toggleBlockquote().run() },
				{ unique: 'code', label: 'Code', execute: () => this.editor?.chain().focus().toggleCodeBlock().run() },
			],
		},
	];

	public editor?: Editor;

	public manifest?: ManifestTiptapToolbarExtension;

	override render() {
		return html`
			<uui-button
				compact
				look="secondary"
				label=${ifDefined(this.manifest?.meta.label)}
				popovertarget="style-select"
				title=${this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : ''}>
				<span>Style select</span>
				<uui-symbol-expand slot="extra" open></uui-symbol-expand>
			</uui-button>
			<umb-cascading-menu-popover id="style-select" placement="bottom-start" .items=${this.#menu}>
			</umb-cascading-menu-popover>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-button-font-weight: normal;

				margin-inline-start: var(--uui-size-space-1);
			}

			uui-button > uui-symbol-expand {
				margin-left: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbTiptapToolbarStyleSelectToolbarElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-style-select-toolbar-element': UmbTiptapToolbarStyleSelectToolbarElement;
	}
}
