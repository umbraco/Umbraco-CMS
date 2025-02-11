// https://github.com/ueberdosis/tiptap/blob/v2.11.5/packages/extension-bubble-menu/src/bubble-menu.ts
// https://github.com/ueberdosis/tiptap/blob/v2.11.5/packages/extension-bubble-menu/src/bubble-menu-plugin.ts

import { Extension } from '@tiptap/core';
import { Editor, isNodeSelection, isTextSelection, posToDOMRect } from '@tiptap/core';
import { EditorState, Plugin, PluginKey } from '@tiptap/pm/state';
import { EditorView } from '@tiptap/pm/view';

export type UmbBubbleMenuOptions = Omit<UmbBubbleMenuPluginProps, 'editor' | 'element'> & {
	/**
	 * The DOM element that contains your menu.
	 * @type {HTMLElement}
	 * @default null
	 */
	element: HTMLElement | null;
};

/**
 * This extension allows you to create a bubble menu.
 * @see https://tiptap.dev/api/extensions/bubble-menu
 */
export const UmbBubbleMenu = Extension.create<UmbBubbleMenuOptions>({
	name: 'umbBubbleMenu',

	addOptions() {
		return {
			element: null,
			tippyOptions: {},
			pluginKey: 'umbBubbleMenu',
			updateDelay: undefined,
			shouldShow: null,
		};
	},

	addProseMirrorPlugins() {
		if (!this.options.element) {
			return [];
		}

		return [
			UmbBubbleMenuPlugin({
				pluginKey: this.options.pluginKey,
				editor: this.editor,
				element: this.options.element,
				updateDelay: this.options.updateDelay,
				shouldShow: this.options.shouldShow,
			}),
		];
	},
});

export interface UmbBubbleMenuPluginProps {
	/**
	 * The plugin key.
	 * @type {PluginKey | string}
	 * @default 'bubbleMenu'
	 */
	pluginKey: PluginKey | string;

	/**
	 * The editor instance.
	 */
	editor: Editor;

	/**
	 * The DOM element that contains your menu.
	 * @type {HTMLElement}
	 * @default null
	 */
	element: HTMLElement;

	/**
	 * The delay in milliseconds before the menu should be updated.
	 * This can be useful to prevent performance issues.
	 * @type {number}
	 * @default 250
	 */
	updateDelay?: number;

	/**
	 * A function that determines whether the menu should be shown or not.
	 * If this function returns `false`, the menu will be hidden, otherwise it will be shown.
	 */
	shouldShow?:
		| ((props: {
				editor: Editor;
				element: HTMLElement;
				view: EditorView;
				state: EditorState;
				oldState?: EditorState;
				from: number;
				to: number;
		  }) => boolean)
		| null;
}

export type UmbBubbleMenuViewProps = UmbBubbleMenuPluginProps & {
	view: EditorView;
};

export class UmbBubbleMenuView {
	public editor: Editor;

	public element: HTMLElement;

	public view: EditorView;

	public preventHide = false;

	//public tippy: Instance | undefined;

	public updateDelay: number;

	private updateDebounceTimer: number | undefined;

	public shouldShow: Exclude<UmbBubbleMenuPluginProps['shouldShow'], null> = ({ view, state, from, to }) => {
		const { doc, selection } = state;
		const { empty } = selection;

		// Sometime check for `empty` is not enough.
		// Doubleclick an empty paragraph returns a node size of 2.
		// So we check also for an empty text size.
		const isEmptyTextBlock = !doc.textBetween(from, to).length && isTextSelection(state.selection);

		// When clicking on a element inside the bubble menu the editor "blur" event
		// is called and the bubble menu item is focussed. In this case we should
		// consider the menu as part of the editor and keep showing the menu
		const isChildOfMenu = this.element.contains(document.activeElement);

		const hasEditorFocus = view.hasFocus() || isChildOfMenu;

		if (!hasEditorFocus || empty || isEmptyTextBlock || !this.editor.isEditable) {
			return false;
		}

		return true;
	};

	popover: HTMLElement;

	constructor({ editor, element, view, updateDelay = 250, shouldShow }: UmbBubbleMenuViewProps) {
		this.editor = editor;
		this.element = element;
		this.view = view;
		this.updateDelay = updateDelay;

		if (shouldShow) {
			this.shouldShow = shouldShow;
		}

		this.element.addEventListener('mousedown', this.mousedownHandler, { capture: true });
		this.view.dom.addEventListener('dragstart', this.dragstartHandler);
		this.editor.on('focus', this.focusHandler);
		this.editor.on('blur', this.blurHandler);

		// Detaches menu content from its current parent
		this.element.remove();
		this.element.style.visibility = 'visible';

		this.popover = document.createElement('uui-popover-container');
		this.popover.style.backgroundColor = 'white';
		this.popover.setAttribute('placement', 'top');
		this.popover.setAttribute('popover', 'manual');
		this.popover.id = 'umb-bubble-menu';
		this.popover.appendChild(this.element);

		this.view.dom.parentElement?.setAttribute('popovertarget', this.popover.id);
		this.view.dom.parentElement?.appendChild(this.popover);
	}

	mousedownHandler = () => {
		this.preventHide = true;
	};

	dragstartHandler = () => {
		this.hide();
	};

	focusHandler = () => {
		// we use `setTimeout` to make sure `selection` is already updated
		setTimeout(() => this.update(this.editor.view));
	};

	blurHandler = ({ event }: { event: FocusEvent }) => {
		if (this.preventHide) {
			this.preventHide = false;

			return;
		}

		if (event?.relatedTarget && this.element.parentNode?.contains(event.relatedTarget as Node)) {
			return;
		}

		if (event?.relatedTarget === this.editor.view.dom) {
			return;
		}

		this.hide();
	};

	update(view: EditorView, oldState?: EditorState) {
		const { state } = view;
		const hasValidSelection = state.selection.from !== state.selection.to;

		if (this.updateDelay > 0 && hasValidSelection) {
			this.handleDebouncedUpdate(view, oldState);
			return;
		}

		const selectionChanged = !oldState?.selection.eq(view.state.selection);
		const docChanged = !oldState?.doc.eq(view.state.doc);

		this.updateHandler(view, selectionChanged, docChanged, oldState);
	}

	handleDebouncedUpdate = (view: EditorView, oldState?: EditorState) => {
		const selectionChanged = !oldState?.selection.eq(view.state.selection);
		const docChanged = !oldState?.doc.eq(view.state.doc);

		if (!selectionChanged && !docChanged) {
			return;
		}

		if (this.updateDebounceTimer) {
			clearTimeout(this.updateDebounceTimer);
		}

		this.updateDebounceTimer = window.setTimeout(() => {
			this.updateHandler(view, selectionChanged, docChanged, oldState);
		}, this.updateDelay);
	};

	updateHandler = (view: EditorView, selectionChanged: boolean, docChanged: boolean, oldState?: EditorState) => {
		const { state, composing } = view;
		const { selection } = state;

		const isSame = !selectionChanged && !docChanged;

		if (composing || isSame) {
			return;
		}

		// support for CellSelections
		const { ranges } = selection;
		const from = Math.min(...ranges.map((range) => range.$from.pos));
		const to = Math.max(...ranges.map((range) => range.$to.pos));

		const shouldShow = this.shouldShow?.({
			editor: this.editor,
			element: this.element,
			view,
			state,
			oldState,
			from,
			to,
		});

		if (!shouldShow) {
			this.hide();

			return;
		}

		this.show();
	};

	show() {
		//console.log('showing bubble menu');
		this.popover.showPopover();
	}

	hide() {
		//console.log('hiding bubble menu');
		this.popover.hidePopover();
	}

	destroy() {
		this.popover.remove();
		this.element.removeEventListener('mousedown', this.mousedownHandler, { capture: true });
		this.view.dom.removeEventListener('dragstart', this.dragstartHandler);
		this.editor.off('focus', this.focusHandler);
		this.editor.off('blur', this.blurHandler);
	}
}

export const UmbBubbleMenuPlugin = (options: UmbBubbleMenuPluginProps) => {
	return new Plugin({
		key: typeof options.pluginKey === 'string' ? new PluginKey(options.pluginKey) : options.pluginKey,
		view: (view) => new UmbBubbleMenuView({ view, ...options }),
	});
};
