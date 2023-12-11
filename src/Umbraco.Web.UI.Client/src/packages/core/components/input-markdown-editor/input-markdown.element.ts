import { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import { marked } from '@umbraco-cms/backoffice/external/marked';
import { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';
import { UmbCodeEditorController, UmbCodeEditorElement, loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { css, html, customElement, query, property, unsafeHTML, when } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUIModalSidebarSize, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UMB_LINK_PICKER_MODAL,
	UMB_MEDIA_TREE_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';

/**
 * @element umb-input-markdown
 * @fires change - when the value of the input changes
 */

@customElement('umb-input-markdown')
export class UmbInputMarkdownElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return this._codeEditor;
	}

	// TODO: Make actions be able to handle multiple selection

	@property({ type: Boolean })
	preview: boolean = false;

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
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
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

			this.#isCodeEditorReady.next(true);
			this.#loadActions();
		} catch (error) {
			console.error(error);
		}
	}

	async #loadActions() {
		//Note: UI Buttons have the keybindings hardcoded in its title. If you change the keybindings here, please update the render as well.
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Heading H1',
			id: 'h1',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Digit1],
			run: () => this._insertAtCurrentLine('# '),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Heading H2',
			id: 'h2',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Digit2],
			run: () => this._insertAtCurrentLine('## '),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Heading H3',
			id: 'h3',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Digit3],
			run: () => this._insertAtCurrentLine('### '),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Heading H4',
			id: 'h4',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Digit4],
			run: () => this._insertAtCurrentLine('#### '),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Heading H5',
			id: 'h5',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Digit5],
			run: () => this._insertAtCurrentLine('##### '),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Heading H6',
			id: 'h6',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Digit6],
			run: () => this._insertAtCurrentLine('###### '),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Bold Text',
			id: 'b',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyB],
			run: () => this._insertBetweenSelection('**', '**', 'Your Bold Text'),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Italic Text',
			id: 'i',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyI],
			run: () => this._insertBetweenSelection('*', '*', 'Your Italic Text'),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Quote',
			id: 'q',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Period],
			run: () => this._insertQuote(),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Ordered List',
			id: 'ol',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Digit7],
			run: () => this._insertAtCurrentLine('1. '),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Unordered List',
			id: 'ul',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.Digit8],
			run: () => this._insertAtCurrentLine('- '),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Code',
			id: 'code',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyE],
			run: () => this._insertBetweenSelection('`', '`', 'Code'),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Fenced Code',
			id: 'fenced-code',
			run: () => this._insertBetweenSelection('```', '```', 'Code'),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Line',
			id: 'line',
			run: () => this._insertLine(),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Link',
			id: 'link',
			keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyK],
			run: () => this._insertLink(),
		});
		this.#editor?.monacoEditor?.addAction({
			label: 'Add Image',
			id: 'image',
			//keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyJ], // What keybinding would be good for image?
			run: () => this._insertMedia(),
			// TODO: Update when media picker is complete.
		});
	}

	private _focusEditor(): void {
		// If we press one of the action buttons manually (which is outside the editor), we need to focus the editor again.
		this.#editor?.monacoEditor?.focus();
	}

	private _insertLink() {
		const selection = this.#editor?.getSelections()[0];
		if (!selection || !this._modalContext) return;

		const selectedValue = this.#editor?.getValueInRange(selection);

		this._focusEditor(); // Focus before opening modal
		const modalContext = this._modalContext.open(UMB_LINK_PICKER_MODAL, {
			data: {
				index: null,
				config: { overlaySize: this.overlaySize },
			},
			value: {
				link: { name: selectedValue },
			},
		});

		modalContext
			?.onSubmit()
			.then((value) => {
				if (!value) return;

				const name = this.localize.term('general_name');
				const url = this.localize.term('general_url');

				this.#editor?.monacoEditor?.executeEdits('', [
					{ range: selection, text: `[${value.link.name || name}](${value.link.url || url})` },
				]);

				if (!value.link.name) {
					this.#editor?.select({
						startColumn: selection.startColumn + 1,
						endColumn: selection.startColumn + 1 + name.length,
						endLineNumber: selection.startLineNumber,
						startLineNumber: selection.startLineNumber,
					});
				} else if (!value.link.url) {
					this.#editor?.select({
						startColumn: selection.startColumn + 3 + value.link.name.length,
						endColumn: selection.startColumn + 3 + value.link.name.length + url.length,
						endLineNumber: selection.startLineNumber,
						startLineNumber: selection.startLineNumber,
					});
				}
			})
			.catch(() => undefined)
			.finally(() => this._focusEditor());
	}

	private _insertMedia() {
		const selection = this.#editor?.getSelections()[0];
		if (!selection) return;

		const alt = this.#editor?.getValueInRange(selection) || 'alt text';

		this._focusEditor(); // Focus before opening modal, otherwise cannot regain focus back after modal
		const modalContext = this._modalContext?.open(UMB_MEDIA_TREE_PICKER_MODAL);

		modalContext
			?.onSubmit()
			.then((value) => {
				if (!value) return;
				const imgUrl = value.selection[0];
				this.#editor?.monacoEditor?.executeEdits('', [
					//TODO: Get the correct media URL
					{
						range: selection,
						text: `![${alt}](${imgUrl ? `${this.serverUrl}'/media/'${imgUrl}` : 'URL'})`,
					},
				]);
				this.#editor?.select({
					startColumn: selection.startColumn + 2,
					endColumn: selection.startColumn + alt.length + 2, // +2 because of ![
					endLineNumber: selection.startLineNumber,
					startLineNumber: selection.startLineNumber,
				});
			})
			.catch(() => undefined)
			.finally(() => this._focusEditor());
	}

	private _insertLine() {
		const selection = this.#editor?.getSelections()[0];
		if (!selection) return;

		const endColumn = this.#editor?.monacoModel?.getLineMaxColumn(selection.endLineNumber) ?? 1;

		if (endColumn === 1) {
			this.#editor?.insertAtPosition('---\n', {
				lineNumber: selection.endLineNumber,
				column: 1,
			});
		} else {
			this.#editor?.insertAtPosition('\n\n---\n', {
				lineNumber: selection.endLineNumber,
				column: endColumn,
			});
		}
		this._focusEditor();
	}

	private _insertBetweenSelection(startValue: string, endValue: string, placeholder?: string) {
		this._focusEditor();
		const selection = this.#editor?.getSelections()[0];
		if (!selection) return;

		const selectedValue = this.#editor?.getValueInRange({
			startLineNumber: selection.startLineNumber,
			endLineNumber: selection.endLineNumber,
			startColumn: selection.startColumn - startValue.length,
			endColumn: selection.endColumn + endValue.length,
		});

		if (
			selectedValue?.startsWith(startValue) &&
			selectedValue.endsWith(endValue) &&
			selectedValue.length > startValue.length + endValue.length
		) {
			//Cancel previous insert
			this.#editor?.select({ ...selection, startColumn: selection.startColumn + startValue.length });
			this.#editor?.monacoEditor?.executeEdits('', [
				{
					range: {
						startColumn: selection.startColumn - startValue.length,
						startLineNumber: selection.startLineNumber,
						endColumn: selection.startColumn,
						endLineNumber: selection.startLineNumber,
					},
					text: '',
				},
				{
					range: {
						startColumn: selection.endColumn + startValue.length,
						startLineNumber: selection.startLineNumber,
						endColumn: selection.endColumn,
						endLineNumber: selection.startLineNumber,
					},
					text: '',
				},
			]);
		} else {
			// Insert
			this.#editor?.insertAtPosition(startValue, {
				lineNumber: selection.startLineNumber,
				column: selection.startColumn,
			});
			this.#editor?.insertAtPosition(endValue, {
				lineNumber: selection.endLineNumber,
				column: selection.endColumn + startValue.length,
			});

			this.#editor?.select({
				startLineNumber: selection.startLineNumber,
				endLineNumber: selection.endLineNumber,
				startColumn: selection.startColumn + startValue.length,
				endColumn: selection.endColumn + startValue.length,
			});
		}

		// if no text were selected when action fired
		if (selection.startColumn === selection.endColumn && selection.startLineNumber === selection.endLineNumber) {
			if (placeholder) {
				this.#editor?.insertAtPosition(placeholder, {
					lineNumber: selection.startLineNumber,
					column: selection.startColumn + startValue.length,
				});
			}

			this.#editor?.select({
				startLineNumber: selection.startLineNumber,
				endLineNumber: selection.endLineNumber,
				startColumn: selection.startColumn + startValue.length,
				endColumn: selection.startColumn + startValue.length + (placeholder?.length ?? 0),
			});
		}
	}

	private _insertAtCurrentLine(value: string) {
		this._focusEditor();
		const selection = this.#editor?.getSelections()[0];
		if (!selection) return;

		const previousLineValue = this.#editor?.getValueInRange({
			...selection,
			startLineNumber: selection.startLineNumber - 1,
		});
		const lineValue = this.#editor?.getValueInRange({ ...selection, startColumn: 1 });

		// Regex: check if the line starts with a positive number followed by dot and a space
		if (lineValue?.startsWith(value) || lineValue?.match(/^[1-9]\d*\.\s.*/)) {
			// Cancel previous insert
			this.#editor?.monacoEditor?.executeEdits('', [
				{
					range: {
						startColumn: 1,
						startLineNumber: selection.startLineNumber,
						endColumn: 1 + value.length,
						endLineNumber: selection.startLineNumber,
					},
					text: '',
				},
			]);
		} else if (value.match(/^[1-9]\d*\.\s.*/) && previousLineValue?.match(/^[1-9]\d*\.\s.*/)) {
			// Check if the PREVIOUS line starts with a positive number followed by dot and a space. If yes, get that number.
			const previousNumber = parseInt(previousLineValue, 10);
			this.#editor?.insertAtPosition(`${previousNumber + 1}. `, {
				lineNumber: selection.startLineNumber,
				column: 1,
			});
		} else {
			// Insert
			this.#editor?.insertAtPosition(value, {
				lineNumber: selection.startLineNumber,
				column: 1,
			});
		}
	}

	private _insertQuote() {
		const selection = this.#editor?.getSelections()[0];
		if (!selection) return;

		let index = selection.startLineNumber;
		for (index; index <= selection.endLineNumber; index++) {
			const line = this.#editor?.getValueInRange({
				startLineNumber: index,
				endLineNumber: index,
				startColumn: 1,
				endColumn: 3,
			});
			if (!line?.startsWith('> ')) {
				this.#editor?.insertAtPosition('> ', {
					lineNumber: index,
					column: 1,
				});
			}
		}
		this._focusEditor();
	}

	private _renderBasicActions() {
		return html`<div>
				<uui-button
					compact
					look="secondary"
					label="Heading"
					title="Heading, &lt;Ctrl+Shift+1&gt;"
					@click=${() => this.#editor?.monacoEditor?.getAction('h1')?.run()}>
					H
				</uui-button>
				<uui-button
					compact
					look="secondary"
					label="Bold"
					title="Bold, &lt;Ctrl+B&gt;"
					@click=${() => this.#editor?.monacoEditor?.getAction('b')?.run()}>
					B
				</uui-button>
				<uui-button
					compact
					look="secondary"
					label="Italic"
					title="Italic, &lt;Ctrl+I&gt;"
					@click=${() => this.#editor?.monacoEditor?.getAction('i')?.run()}>
					I
				</uui-button>
			</div>
			<div>
				<uui-button
					compact
					look="secondary"
					label="Quote"
					title="Quote, &lt;Ctrl+Shift+.&gt;"
					@click=${() => this.#editor?.monacoEditor?.getAction('q')?.run()}>
					<uui-icon name="icon-quote"></uui-icon>
				</uui-button>
				<uui-button
					compact
					look="secondary"
					label="Ordered List"
					title="Ordered List, &lt;Ctrl+Shift+7&gt;"
					@click=${() => this.#editor?.monacoEditor?.getAction('ol')?.run()}>
					<uui-icon name="icon-ordered-list"></uui-icon>
				</uui-button>
				<uui-button
					compact
					look="secondary"
					label="Unordered List"
					title="Unordered List, &lt;Ctrl+Shift+8&gt;"
					@click=${() => this.#editor?.monacoEditor?.getAction('ul')?.run()}>
					<uui-icon name="icon-bulleted-list"></uui-icon>
				</uui-button>
			</div>
			<div>
				<uui-button
					compact
					look="secondary"
					label="Code"
					title="Code, &lt;Ctrl+E&gt;"
					@click=${() => this.#editor?.monacoEditor?.getAction('code')?.run()}>
					<uui-icon name="icon-code"></uui-icon>
				</uui-button>
				<uui-button
					compact
					look="secondary"
					label="Line"
					title="Line"
					@click=${() => this.#editor?.monacoEditor?.getAction('line')?.run()}>
					<uui-icon name="icon-width"></uui-icon>
				</uui-button>
				<uui-button
					compact
					look="secondary"
					label="Link"
					title="Link, &lt;Ctrl+K&gt;"
					@click=${() => this.#editor?.monacoEditor?.getAction('link')?.run()}>
					<uui-icon name="icon-link"></uui-icon>
				</uui-button>
				<uui-button
					compact
					look="secondary"
					label="Image"
					title="Image"
					@click=${() => this.#editor?.monacoEditor?.getAction('image')?.run()}>
					<uui-icon name="icon-picture"></uui-icon>
				</uui-button>
			</div>
			<div>
				<uui-button
					compact
					label="Press F1 for all actions"
					title="Press F1 for all actions"
					@click=${() => {
						this._focusEditor();
						this.#editor?.monacoEditor?.trigger('', 'editor.action.quickCommand', '');
					}}>
					<uui-key>F1</uui-key>
				</uui-button>
			</div>`;
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
		return html` <div id="actions">${this._renderBasicActions()}</div>
			<umb-code-editor
				language="markdown"
				.code=${this.value as string}
				@keypress=${this.onKeyPress}
				@input=${this.#onInput}
				theme="umb-light"></umb-code-editor>
			${when(this.preview && this.value, () => this.renderPreview(this.value as string))}`;
	}

	renderPreview(markdown: string) {
		const markdownAsHtml = marked.parse(markdown);
		const sanitizedHtml = markdownAsHtml ? DOMPurify.sanitize(markdownAsHtml) : '';
		return html`<uui-scroll-container id="preview"> ${unsafeHTML(sanitizedHtml)} </uui-scroll-container>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
			#actions {
				background-color: var(--uui-color-background-alt);
				display: flex;
				gap: var(--uui-size-6);
			}

			#preview {
				max-height: 400px;
			}

			#actions div {
				display: flex;
				gap: var(--uui-size-1);
			}

			#actions div:last-child {
				margin-left: auto;
			}

			umb-code-editor {
				height: 200px;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-divider-emphasis);
			}

			uui-button {
				width: 50px;
			}

			blockquote {
				border-left: 2px solid var(--uui-color-default-emphasis);
				margin-inline: 0;
				padding-inline: var(--uui-size-3);
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
export default UmbInputMarkdownElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-markdown': UmbInputMarkdownElement;
	}
}
