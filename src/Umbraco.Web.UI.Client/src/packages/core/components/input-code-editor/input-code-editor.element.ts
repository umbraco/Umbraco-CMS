import { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import { marked } from '@umbraco-cms/backoffice/external/marked';
import { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';
import type { UmbCodeEditorController, UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { css, html, customElement, query, property, unsafeHTML, when } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, type UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import {
	UMB_LINK_PICKER_MODAL,
	UMB_MEDIA_TREE_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
} from '@umbraco-cms/backoffice/modal';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

/**
 * @element umb-input-code-editor
 * @fires change - when the value of the input changes
 */

@customElement('umb-input-code-editor')
export class UmbInputCodeEditorElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return this._codeEditor;
	}

	@property({ type: String })
	language: string = 'HTML';

	@property()
	overlaySize?: UUIModalSidebarSize;

	#isCodeEditorReady = new UmbBooleanState(false);
	#editor?: UmbCodeEditorController;

	@query('umb-code-editor')
	_codeEditor?: UmbCodeEditorElement;

	private _modalContext?: UmbModalManagerContext;

	private serverUrl?: string;

	constructor() {
		super();
		this.#loadCodeEditor();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this._modalContext = instance;
		});
		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.serverUrl = instance.getServerUrl();
		});
	}

	async #loadCodeEditor() {
		try {
			await loadCodeEditor();

			this.#editor = this._codeEditor?.editor;

			this.#editor?.updateOptions({
				lineNumbers: false,
				minimap: false,
				folding: false,
			}); // Prefer to update options before showing the editor, to avoid seeing the changes in the UI.

			this.#isCodeEditorReady.setValue(true);
		} catch (error) {
			console.error(error);
		}
	}

	private _focusEditor(): void {
		// If we press one of the action buttons manually (which is outside the editor), we need to focus the editor again.
		this.#editor?.monacoEditor?.focus();
	}

	onKeyPress(e: KeyboardEvent) {
		if (e.key !== 'Enter') return;
		//TODO: Tab does not seem to trigger keyboard events. We need to make some logic for ordered and unordered lists when tab is being used.

		const selection = this.#editor?.getSelections()[0];
		if (!selection) return;

		const lineValue = this.#editor?.getValueInRange({ ...selection, startColumn: 1 }).trimStart();
		if (!lineValue) return;

		if (lineValue.startsWith('- ') && lineValue.length > 2) {
			requestAnimationFrame(() => this.#editor?.insert('- '));
		} else if (lineValue.match(/^[1-9]\d*\.\s.*/) && lineValue.length > 3) {
			const previousNumber = parseInt(lineValue, 10);
			requestAnimationFrame(() => this.#editor?.insert(`${previousNumber + 1}. `));
		}
	}

	#onInput(e: CustomEvent) {
		e.stopPropagation();
		this.value = this.#editor?.monacoEditor?.getValue() ?? '';
		this.dispatchEvent(new CustomEvent('change'));
	}

	render() {
		return html`
			<umb-code-editor
				language="${this.language}"
				.code=${this.value as string}
				@keypress=${this.onKeyPress}
				@input=${this.#onInput}
				theme="umb-light"></umb-code-editor>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			umb-code-editor {
				height: 200px;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-divider-emphasis);
			}

			p > code,
			pre {
				border: 1px solid var(--uui-color-divider-emphasis);
				border-radius: var(--uui-border-radius);
				padding: 0 var(--uui-size-1);
				background-color: var(--uui-color-background);
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-palette-cocoa-black);
			}
		`,
	];
}
export default UmbInputCodeEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-code-editor': UmbInputCodeEditorElement;
	}
}
