import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor, UmbTiptapBubbleMenuElement } from '@umbraco-cms/backoffice/external/tiptap';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
@customElement('umb-tiptap-table-row-menu')
export class UmbTiptapTableRowMenuElement extends UmbLitElement implements UmbTiptapBubbleMenuElement {
	@property({ attribute: false })
	editor?: Editor;

	#onAddRowBefore = () => this.editor?.chain().focus().addRowBefore().run();
	#onAddRowAfter = () => this.editor?.chain().focus().addRowAfter().run();
	#onDeleteRow = () => this.editor?.chain().focus().deleteRow().run();

	override render() {
		return html`
			<uui-menu-item label="Add row before" @click=${this.#onAddRowBefore}>
				<uui-icon slot="icon" name="icon-page-up"></uui-icon>
			</uui-menu-item>
			<uui-menu-item label="Add row after" @click=${this.#onAddRowAfter}>
				<uui-icon slot="icon" name="icon-page-down"></uui-icon>
			</uui-menu-item>
			<uui-menu-item label="Delete row" @click=${this.#onDeleteRow}>
				<uui-icon slot="icon" name="icon-trash"></uui-icon>
			</uui-menu-item>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-menu-item-flat-structure: 1;

				display: flex;
				flex-direction: column;

				background-color: var(--uui-color-surface);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
			}
		`,
	];
}

export default UmbTiptapTableRowMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-table-row-menu': UmbTiptapTableRowMenuElement;
	}
}
