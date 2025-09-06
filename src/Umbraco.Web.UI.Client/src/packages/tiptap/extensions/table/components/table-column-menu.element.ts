import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor, UmbTiptapBubbleMenuElement } from '@umbraco-cms/backoffice/external/tiptap';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
@customElement('umb-tiptap-table-column-menu')
export class UmbTiptapTableColumnMenuElement extends UmbLitElement implements UmbTiptapBubbleMenuElement {
	@property({ attribute: false })
	editor?: Editor;

	#onAddColumnBefore = () => this.editor?.chain().focus().addColumnBefore().run();
	#onAddColumnAfter = () => this.editor?.chain().focus().addColumnAfter().run();
	#onDeleteColumn = () => this.editor?.chain().focus().deleteColumn().run();

	override render() {
		return html`
			<uui-menu-item label="Add column before" @click=${this.#onAddColumnBefore}>
				<uui-icon slot="icon" name="icon-navigation-first"></uui-icon>
			</uui-menu-item>
			<uui-menu-item label="Add column after" @click=${this.#onAddColumnAfter}>
				<uui-icon slot="icon" name="icon-tab-key"></uui-icon>
			</uui-menu-item>
			<uui-menu-item label="Delete column" @click=${this.#onDeleteColumn}>
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

export default UmbTiptapTableColumnMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-table-column-menu': UmbTiptapTableColumnMenuElement;
	}
}
