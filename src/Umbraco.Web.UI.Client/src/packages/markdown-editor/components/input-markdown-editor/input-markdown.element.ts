import { css, customElement, html, property, query, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { marked } from '@umbraco-cms/backoffice/external/marked';
import { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_MEDIA_PICKER_MODAL, UmbMediaUrlRepository } from '@umbraco-cms/backoffice/media';
import { UmbCodeEditorLoadedEvent } from '@umbraco-cms/backoffice/code-editor';
import type { UmbCodeEditorController, UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

const elementName = 'umb-input-markdown';

/**
 * @element umb-input-markdown
 * @fires change - when the value of the input changes
 */
@customElement(elementName)
export class UmbInputMarkdownElement extends UmbFormControlMixin(UmbLitElement, '') {
	protected override getFormElement() {
		return this._codeEditor;
	}

	// TODO: Make actions be able to handle multiple selection

	@property({ type: Boolean })
	preview: boolean = false;

	@property()
	overlaySize?: UUIModalSidebarSize;

	#editor?: UmbCodeEditorController;

	@query('umb-code-editor', true)
	private _codeEditor?: UmbCodeEditorElement;

	@state()
	private _actionExtensions: Array<monaco.editor.IActionDescriptor> = [];

	#mediaUrlRepository = new UmbMediaUrlRepository(this);

	#onCodeEditorLoaded(event: UmbCodeEditorLoadedEvent) {
		if (event.type !== UmbCodeEditorLoadedEvent.TYPE) return;

		try {
			this.#editor = this._codeEditor?.editor;

			this.#editor?.updateOptions({
				lineNumbers: false,
				minimap: false,
				folding: false,
			}); // Prefer to update options before showing the editor, to avoid seeing the changes in the UI.

			// TODO: make all action into extensions
			this.observe(umbExtensionsRegistry.byType('monacoMarkdownEditorAction'), (manifests) => {
				manifests.forEach(async (manifest) => {
					const api = await createExtensionApi(this, manifest, [this]);
					const action: monaco.editor.IActionDescriptor = {
						id: api.getUnique(),
						label: api.getLabel(),
						keybindings: api.getKeybindings(),
						run: async () => await api.execute({ editor: this.#editor, overlaySize: this.overlaySize }),
					};
					this.#editor?.monacoEditor?.addAction(action);
					this._actionExtensions.push(action);
					this.requestUpdate('_actionExtensions');
				});
			});

			this.#loadActions();
		} catch (error) {
			console.error(error);
		}
	}

	#loadActions() {
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
			label: 'Add Image',
			id: 'image',
			//keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyJ], // What keybinding would be good for image?
			run: () => this.#insertMedia(),
			// TODO: Update when media picker is complete.
		});
	}

	#onActionClick(event: any, action: monaco.editor.IActionDescriptor) {
		event.stopPropagation();
		const hasAction = this.#editor?.monacoEditor?.getAction(action.id);
		if (!hasAction) throw new Error(`Action ${action.id} not found in the editor.`);
		this.#editor?.monacoEditor?.getAction(action.id)?.run();
	}

	private _focusEditor(): void {
		// If we press one of the action buttons manually (which is outside the editor), we need to focus the editor again.
		this.#editor?.monacoEditor?.focus();
	}

	async #insertMedia() {
		const selection = this.#editor?.getSelections()[0];
		if (!selection) return;

		const alt = this.#editor?.getValueInRange(selection) || 'enter image description here';

		this._focusEditor(); // Focus before opening modal, otherwise cannot regain focus back after modal

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_MEDIA_PICKER_MODAL);

		modalContext
			?.onSubmit()
			.then(async (value) => {
				if (!value) return;

				const uniques = value.selection;
				const { data: mediaUrls } = await this.#mediaUrlRepository.requestItems(uniques);
				const mediaUrl = mediaUrls?.length ? (mediaUrls[0].url ?? 'URL') : 'URL';

				this.#editor?.monacoEditor?.executeEdits('', [
					{
						range: selection,
						text: `![${alt}](${mediaUrl})`,
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

	#onKeyPress(e: KeyboardEvent) {
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
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			${this.#renderToolbar()}
			<umb-code-editor
				language="markdown"
				.code=${this.value as string}
				@input=${this.#onInput}
				@keypress=${this.#onKeyPress}
				@loaded=${this.#onCodeEditorLoaded}>
			</umb-code-editor>
			${this.#renderPreview()}
		`;
	}

	#renderToolbar() {
		return html`
			<div id="toolbar">
				<uui-button-group>
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
				</uui-button-group>

				<uui-button-group>
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
				</uui-button-group>
				<uui-button-group>
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
						label="Image"
						title="Image"
						@click=${() => this.#editor?.monacoEditor?.getAction('image')?.run()}>
						<uui-icon name="icon-picture"></uui-icon>
					</uui-button>
				</uui-button-group>

				<uui-button-group>
					${this._actionExtensions.map(
						(action) => html`
							<uui-button
								compact
								look="secondary"
								label=${action.label}
								@click=${(event: any) => this.#onActionClick(event, action)}>
								<uui-icon name="icon-link"></uui-icon>
							</uui-button>
						`,
					)}
				</uui-button-group>

				<uui-button-group>
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
				</uui-button-group>
			</div>
		`;
	}

	#renderPreview() {
		if (!this.preview || !this.value) return;
		const markdownAsHtml = marked.parse(this.value as string) as string;
		const sanitizedHtml = markdownAsHtml ? DOMPurify.sanitize(markdownAsHtml) : '';
		return html`<uui-scroll-container id="preview">${unsafeHTML(sanitizedHtml)}</uui-scroll-container>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			#toolbar {
				background-color: var(--uui-color-background-alt);
				display: flex;
				flex-wrap: wrap;
				gap: var(--uui-size-2);
				margin-bottom: var(--uui-size-2);
			}

			umb-code-editor {
				height: 200px;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-divider-emphasis);
			}

			uui-button {
				width: 50px;
			}

			#preview {
				max-height: 400px;
			}

			#preview blockquote {
				border-left: 2px solid var(--uui-color-default-emphasis);
				margin-inline: 0;
				padding-inline: var(--uui-size-3);
			}

			#preview img {
				max-width: 100%;
			}

			#preview hr {
				border: none;
				border-bottom: 1px solid var(--uui-palette-cocoa-black);
			}

			#preview p > code,
			#preview pre {
				border: 1px solid var(--uui-color-divider-emphasis);
				border-radius: var(--uui-border-radius);
				padding: 0 var(--uui-size-1);
				background-color: var(--uui-color-background);
			}
		`,
	];
}
export default UmbInputMarkdownElement;

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbInputMarkdownElement;
	}
}
