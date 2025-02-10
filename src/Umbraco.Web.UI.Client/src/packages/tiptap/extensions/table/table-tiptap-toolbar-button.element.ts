import { UmbTiptapToolbarButtonElement } from '../../components/toolbar/tiptap-toolbar-button.element.js';
import type UmbTiptapToolbarTableExtensionApi from './table.tiptap-toolbar-api.js';
import { css, customElement, html, ifDefined, query, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

type UmbTiptapToolbarTableMenuItem = {
	label: string;
	icon?: string;
	execute: (editor: Editor) => void;
};

@customElement('umb-table-tiptap-toolbar-button')
export class UmbTiptapToolbarTableToolbarButtonElement extends UmbTiptapToolbarButtonElement {
	#menu: Array<UmbTiptapToolbarTableMenuItem> = [
		{
			label: 'Insert table',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().insertTable({ rows: 2, cols: 2, withHeaderRow: false }).run(),
		},
		{
			label: 'Add column before',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().addColumnBefore().run(),
		},
		{
			label: 'Add column after',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().addColumnAfter().run(),
		},
		{
			label: 'Delete column',
			icon: 'icon-trash',
			execute: (editor: Editor) => editor.chain().focus().deleteColumn().run(),
		},
		{
			label: 'Add row before',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().addRowBefore().run(),
		},
		{
			label: 'Add row after',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().addRowAfter().run(),
		},
		{
			label: 'Delete row',
			icon: 'icon-trash',
			execute: (editor: Editor) => editor.chain().focus().deleteRow().run(),
		},
		{
			label: 'Delete table',
			icon: 'icon-trash',
			execute: (editor: Editor) => editor.chain().focus().deleteTable().run(),
		},
		{
			label: 'Merge cells',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().mergeCells().run(),
		},
		{
			label: 'Split cell',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().splitCell().run(),
		},
		{
			label: 'Toggle header column',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().toggleHeaderColumn().run(),
		},
		{
			label: 'Toggle header row',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().toggleHeaderRow().run(),
		},
		{
			label: 'Toggle header cell',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().toggleHeaderCell().run(),
		},
		{
			label: 'Merge or split',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().mergeOrSplit().run(),
		},
	];

	@query('#table-menu-popover')
	private _popover?: UUIPopoverContainerElement;

	override api?: UmbTiptapToolbarTableExtensionApi;

	#onClick(item: UmbTiptapToolbarTableMenuItem) {
		if (!item.execute || !this.editor) return;

		item.execute(this.editor);

		setTimeout(() => {
			// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this._popover?.hidePopover();
		}, 100);
	}

	override render() {
		return html`
			<uui-button
				compact
				look=${this.isActive ? 'outline' : 'default'}
				label=${ifDefined(this.manifest?.meta.label)}
				popovertarget="table-menu-popover"
				title=${this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : ''}>
				${when(
					this.manifest?.meta.icon,
					(icon) => html`<umb-icon name=${icon}></umb-icon>`,
					() => html`<span>${this.manifest?.meta.label}</span>`,
				)}
			</uui-button>

			<uui-popover-container id="table-menu-popover" placement="bottom-start">
				<umb-popover-layout>
					<uui-scroll-container>
						${repeat(
							this.#menu,
							(item) => item.label,
							(item) => html`
								<uui-menu-item label=${item.label} @click-label=${() => this.#onClick(item)}>
									${when(item.icon, (icon) => html`<umb-icon slot="icon" name=${icon}></umb-icon>`)}
								</uui-menu-item>
							`,
						)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}

			uui-scroll-container {
				max-height: 500px;
			}
		`,
	];
}

export { UmbTiptapToolbarTableToolbarButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-table-tiptap-toolbar-button': UmbTiptapToolbarTableToolbarButtonElement;
	}
}
