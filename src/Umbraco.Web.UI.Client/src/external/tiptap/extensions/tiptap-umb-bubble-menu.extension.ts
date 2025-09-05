import type { UUIPopoverContainerElement } from '../../uui/index.js';
import { Extension } from '@tiptap/core';
import { Editor } from '@tiptap/core';
import { EditorState, Plugin, PluginKey } from '@tiptap/pm/state';
import { EditorView } from '@tiptap/pm/view';
import type { PluginView } from '@tiptap/pm/state';

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export interface UmbTiptapBubbleMenuElement extends HTMLElement {
	editor?: Editor;
	menuAlias?: string;
}

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export type UmbBubbleMenuPluginProps = {
	unique: string;
	placement?: UUIPopoverContainerElement['placement'];
	elementName?: string | null;
	menuAlias?: string;
	shouldShow?:
		| ((props: { editor: Editor; view: EditorView; state: EditorState; from: number; to: number }) => boolean)
		| null;
};

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export type UmbBubbleMenuOptions = UmbBubbleMenuPluginProps;

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export const UmbBubbleMenu = Extension.create<UmbBubbleMenuOptions>({
	name: 'umbBubbleMenu',

	addOptions() {
		return {
			unique: 'umb-tiptap-menu',
			placement: 'top',
			elementName: null,
			shouldShow: null,
		};
	},

	addProseMirrorPlugins() {
		if (!this.options.unique || !this.options.elementName) {
			return [];
		}

		return [
			UmbBubbleMenuPlugin(this.editor, {
				unique: this.options.unique,
				placement: this.options.placement,
				elementName: this.options.elementName,
				menuAlias: this.options.menuAlias,
				shouldShow: this.options.shouldShow,
			}),
		];
	},
});

class UmbBubbleMenuPluginView implements PluginView {
	#editor: Editor;

	#popover: UUIPopoverContainerElement;

	#shouldShow: UmbBubbleMenuPluginProps['shouldShow'];

	constructor(editor: Editor, view: EditorView, props: UmbBubbleMenuPluginProps) {
		this.#editor = editor;

		this.#shouldShow = props.shouldShow ?? null;

		this.#popover = document.createElement('uui-popover-container') as UUIPopoverContainerElement;
		this.#popover.id = props.unique;
		this.#popover.setAttribute('placement', props.placement ?? 'top');
		this.#popover.setAttribute('popover', 'manual');

		if (props.elementName) {
			const menu = document.createElement(props.elementName) as UmbTiptapBubbleMenuElement;
			menu.editor = editor;
			menu.menuAlias = props.menuAlias;
			this.#popover.appendChild(menu);
		}

		view.dom.parentNode?.appendChild(this.#popover);

		this.update(view, null);
	}

	update(view: EditorView, prevState: EditorState | null) {
		const editor = this.#editor;

		const { state } = view;
		const { selection } = state;

		const { ranges } = selection;
		const from = Math.min(...ranges.map((range) => range.$from.pos));
		const to = Math.max(...ranges.map((range) => range.$to.pos));

		const shouldShow = this.#shouldShow?.({ editor, view, state, from, to });

		if (!shouldShow) {
			this.#popover.hidePopover();
		} else {
			this.#popover.showPopover();
		}
	}

	destroy() {
		this.#popover.remove();
	}
}

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export const UmbBubbleMenuPlugin = (editor: Editor, props: UmbBubbleMenuPluginProps) => {
	return new Plugin({
		view(editorView) {
			return new UmbBubbleMenuPluginView(editor, editorView, props);
		},
	});
};
