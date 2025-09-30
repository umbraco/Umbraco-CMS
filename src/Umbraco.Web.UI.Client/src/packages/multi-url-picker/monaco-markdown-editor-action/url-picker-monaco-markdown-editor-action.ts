import { UMB_LINK_PICKER_MODAL } from '../link-picker-modal/link-picker-modal.token.js';
import { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

export class UmbUrlPickerMonacoMarkdownEditorAction extends UmbControllerBase {
	#localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	getUnique() {
		return 'Umb.MonacoMarkdownEditorAction.UrlPicker';
	}

	getLabel() {
		return this.#localize.term('general_insertLink');
	}

	getKeybindings() {
		return [monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyK];
	}

	async execute({ editor, overlaySize }: { editor: any; overlaySize: UUIModalSidebarSize }) {
		if (!editor) throw new Error('Editor not found');
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) throw new Error('Modal manager not found');

		const selection = editor?.getSelections()[0];
		if (!selection) return;

		const selectedValue = editor?.getValueInRange(selection);
		editor.monacoEditor?.focus();

		const modalContext = modalManager.open(this, UMB_LINK_PICKER_MODAL, {
			modal: { size: overlaySize },
			data: {
				index: null,
				isNew: selectedValue === '',
				config: {},
			},
			value: {
				link: { name: selectedValue },
			},
		});

		modalContext
			?.onSubmit()
			.then((value) => {
				if (!value) return;

				const name = this.#localize.term('general_name');
				const url = this.#localize.term('general_url');

				editor.monacoEditor?.executeEdits('', [
					{ range: selection, text: `[${value.link.name || name}](${value.link.url || url})` },
				]);

				if (!value.link.name) {
					editor.select({
						startColumn: selection.startColumn + 1,
						endColumn: selection.startColumn + 1 + name.length,
						endLineNumber: selection.startLineNumber,
						startLineNumber: selection.startLineNumber,
					});
				} else if (!value.link.url) {
					editor.select({
						startColumn: selection.startColumn + 3 + value.link.name.length,
						endColumn: selection.startColumn + 3 + value.link.name.length + url.length,
						endLineNumber: selection.startLineNumber,
						startLineNumber: selection.startLineNumber,
					});
				}
			})
			.catch(() => undefined)
			.finally(() => editor.monacoEditor?.focus());
	}
}

export { UmbUrlPickerMonacoMarkdownEditorAction as api };
