import * as monaco from 'monaco-editor';
import {
	CodeEditorConstructorOptions,
	CodeEditorSearchOptions,
	CodeEditorTheme,
	UmbCodeEditorCursorPosition,
	UmbCodeEditorCursorPositionChangedEvent,
	UmbCodeEditorCursorSelectionChangedEvent,
	UmbCodeEditorHost,
	UmbCodeEditorRange,
	UmbCodeEditorSelection,
} from './code-editor.model';
import themes from './themes';
import { UmbChangeEvent, UmbInputEvent } from '@umbraco-cms/backoffice/events';

//TODO - consider firing change event on blur

/**
 * This is a wrapper class for the [monaco editor](https://microsoft.github.io/monaco-editor). It exposes some of the monaco editor API. It also handles the creation of the monaco editor.
 * It allows access to the entire monaco editor object through `monacoEditor` property, but mind the fact that editor might be swapped in the future for a different library, so use on your own responsibility.
 * Through the UmbCodeEditorHost interface it can be used in a custom element.
 * By using monaco library directly you can access the entire monaco API along with code completions, actions etc. This class creates some level of abstraction over the monaco editor. It only provides basic functionality, that should be enough for most of the use cases and should be possible to implement with any code editor library.
 *
 * Current issues: [shadow DOM related issues](https://github.com/microsoft/monaco-editor/labels/editor-shadow-dom) #3217  currently fixed by a hack , [razor syntax highlight](https://github.com/microsoft/monaco-editor/issues/1997)
 *
 *
 * @export
 * @class UmbCodeEditor
 */
export class UmbCodeEditorController {
	#host: UmbCodeEditorHost;
	#editor?: monaco.editor.IStandaloneCodeEditor;
	/**
	 * The monaco editor object. This is the actual monaco editor object. It is exposed for advanced usage, but mind the fact that editor might be swapped in the future for a different library, so use on your own responsibility. For more information see [monaco editor API](https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.IStandaloneCodeEditor.html).
	 *
	 * @readonly
	 * @memberof UmbCodeEditor
	 */
	get monacoEditor() {
		return this.#editor;
	}

	#options: CodeEditorConstructorOptions = {};
	/**
	 * The options used to create the editor.
	 *
	 * @readonly
	 * @type {CodeEditorConstructorOptions}
	 * @memberof UmbCodeEditor
	 */
	get options(): CodeEditorConstructorOptions {
		return this.#options;
	}

	#defaultMonacoOptions: monaco.editor.IStandaloneEditorConstructionOptions = {
		automaticLayout: true,
		scrollBeyondLastLine: false,
		scrollbar: {
			verticalScrollbarSize: 5,
		},
		// disable this, as it does not work with shadow dom properly.
		colorDecorators: false,
	};

	#position: UmbCodeEditorCursorPosition | null = null;
	/**
	 * Provides the current position of the cursor.
	 *
	 * @readonly
	 * @memberof UmbCodeEditor
	 */
	get position() {
		return this.#position;
	}
	#secondaryPositions: UmbCodeEditorCursorPosition[] = [];
	/**
	 * Provides positions of all the secondary cursors.
	 *
	 * @readonly
	 * @memberof UmbCodeEditor
	 */
	get secondaryPositions() {
		return this.#secondaryPositions;
	}

	/**
	 * Provides the current value of the editor.
	 *
	 * @memberof UmbCodeEditor
	 */
	get value() {
		if (!this.#editor) return '';
		const value = this.#editor.getValue();
		return value;
	}

	set value(newValue: string) {
		if (!this.#editor) throw new Error('Editor object not found');

		const oldValue = this.value;
		if (newValue !== oldValue) {
			this.#editor.setValue(newValue);
		}
	}
	/**
	 * Provides the current model of the editor. For advanced usage. Bare in mind that in case of the monaco library being swapped in the future, this might not be available. For more information see [monaco editor model API](https://microsoft.github.io/monaco-editor/docs.html#interfaces/editor.ITextModel.html).
	 *
	 * @readonly
	 * @memberof UmbCodeEditor
	 */
	get monacoModel() {
		if (!this.#editor) return null;
		return this.#editor.getModel();
	}
	/**
	 * Creates an instance of UmbCodeEditor. You should instantiate this class through the `UmbCodeEditorHost` interface and that should happen when inside DOM nodes of the host container are available, otherwise the editor will not be able to initialize, for example in lit `firstUpdated` lifecycle hook. It will make host emit change and input events when the value of the editor changes.
	 * @param {UmbCodeEditorHost} host
	 * @param {CodeEditorConstructorOptions} [options]
	 * @memberof UmbCodeEditor
	 */
	constructor(host: UmbCodeEditorHost, options?: CodeEditorConstructorOptions) {
		this.#options = { ...options };
		this.#host = host;
		this.#registerThemes();
		this.#createEditor(options);
	}

	#registerThemes() {
		Object.entries(themes).forEach(([name, theme]) => {
			this.#defineTheme(name, theme);
		});
	}

	#defineTheme(name: string, theme: monaco.editor.IStandaloneThemeData) {
		monaco.editor.defineTheme(name, theme);
	}

	#initiateEvents() {
		this.#editor?.onDidChangeModelContent(() => {
			this.#host.code = this.value ?? '';
			this.#host.dispatchEvent(new UmbInputEvent());
		});

		this.#editor?.onDidChangeModel(() => {
			this.#host.dispatchEvent(new UmbChangeEvent());
		});
		this.#editor?.onDidChangeCursorPosition((e) => {
			this.#position = e.position;
			this.#secondaryPositions = e.secondaryPositions;
		});
	}

	#mapOptions(options: CodeEditorConstructorOptions): monaco.editor.IStandaloneEditorConstructionOptions {
		const hasLineNumbers = Object.prototype.hasOwnProperty.call(options, 'lineNumbers');
		const hasMinimap = Object.prototype.hasOwnProperty.call(options, 'minimap');
		const hasLightbulb = Object.prototype.hasOwnProperty.call(options, 'lightbulb');

		return {
			...options,
			lineNumbers: hasLineNumbers ? (options.lineNumbers ? 'on' : 'off') : undefined,
			minimap: hasMinimap ? (options.minimap ? { enabled: true } : { enabled: false }) : undefined,
			lightbulb: hasLightbulb ? (options.lightbulb ? { enabled: true } : { enabled: false }) : undefined,
		};
	}
	/**
	 * Updates the options of the editor. This is useful for updating the options after the editor has been created.
	 *
	 * @param {CodeEditorConstructorOptions} newOptions
	 * @memberof UmbCodeEditor
	 */
	updateOptions(newOptions: CodeEditorConstructorOptions) {
		if (!this.#editor) throw new Error('Editor object not found');
		this.#options = { ...this.#options, ...newOptions };
		this.#editor.updateOptions(this.#mapOptions(newOptions));
	}

	#createEditor(options: CodeEditorConstructorOptions = {}) {
		if (!this.#host.container) throw new Error('Container not found');
		if (this.#host.container.hasChildNodes()) throw new Error('Editor container should be empty');

		const mergedOptions = { ...this.#defaultMonacoOptions, ...this.#mapOptions(options) };

		this.#editor = monaco.editor.create(this.#host.container, {
			...mergedOptions,
			value: this.#host.code ?? '',
			language: this.#host.language,
			theme: this.#host.theme,
			readOnly: this.#host.readonly,
			ariaLabel: this.#host.label,
		});
		this.#initiateEvents();
	}
	/**
	 * Provides the current selections of the editor.
	 *
	 * @return {*}  {UmbCodeEditorSelection[]}
	 * @memberof UmbCodeEditor
	 */
	getSelections(): UmbCodeEditorSelection[] {
		if (!this.#editor) return [];
		return this.#editor.getSelections() ?? [];
	}
	/**
	 * Provides the current positions of the cursor or multiple cursors.
	 *
	 * @return {*}  {(UmbCodeEditorCursorPosition | null)}
	 * @memberof UmbCodeEditor
	 */
	getPositions(): UmbCodeEditorCursorPosition | null {
		if (!this.#editor) return null;
		return this.#editor.getPosition();
	}
	/**
	 * Inserts text at the current cursor position or multiple cursor positions.
	 *
	 * @param {string} text
	 * @memberof UmbCodeEditor
	 */
	insert(text: string) {
		if (!this.#editor) throw new Error('Editor object not found');
		const selections = this.#editor.getSelections() ?? [];
		if (selections?.length > 0) {
			this.#editor.executeEdits(
				null,
				selections.map((selection) => ({ range: selection, text }))
			);
		}
	}
	/**
	 * Looks for a string or matching strings in the editor and returns the ranges of the found strings. Can use regex, case sensitive and more. If you want regex set the isRegex to true in the options.
	 *
	 * @param {string} searchString
	 * @param {CodeEditorSearchOptions} [searchOptions=<CodeEditorSearchOptions>{}]
	 * @return {*}  {UmbCodeEditorRange[]}
	 * @memberof UmbCodeEditor
	 */
	find(
		searchString: string,
		searchOptions: CodeEditorSearchOptions = <CodeEditorSearchOptions>{}
	): UmbCodeEditorRange[] {
		if (!this.#editor) throw new Error('Editor object not found');
		const defaultOptions = {
			searchOnlyEditableRange: false,

			isRegex: false,

			matchCase: false,

			wordSeparators: null,

			captureMatches: false,
		};

		const { searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches } = {
			...defaultOptions,
			...searchOptions,
		};
		return (
			this.monacoModel
				?.findMatches(searchString, searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches)
				.map((findMatch) => ({
					startLineNumber: findMatch.range.startLineNumber,
					startColumn: findMatch.range.startColumn,
					endLineNumber: findMatch.range.endLineNumber,
					endColumn: findMatch.range.endColumn,
				})) ?? []
		);
	}
	/**
	 * Returns the value of the editor for a given range.
	 *
	 * @param {UmbCodeEditorRange} range
	 * @return {*}  {string}
	 * @memberof UmbCodeEditor
	 */
	getValueInRange(range: UmbCodeEditorRange): string {
		if (!this.#editor) throw new Error('Editor object not found');
		return this.monacoModel?.getValueInRange(range) ?? '';
	}
	/**
	 * Inserts text at a given position.
	 *
	 * @param {string} text
	 * @param {UmbCodeEditorCursorPosition} position
	 * @memberof UmbCodeEditor
	 */
	insertAtPosition(text: string, position: UmbCodeEditorCursorPosition) {
		if (!this.#editor) throw new Error('Editor object not found');
		this.#editor.executeEdits(null, [
			{
				range: {
					startLineNumber: position.lineNumber,
					startColumn: position.column,
					endLineNumber: position.lineNumber,
					endColumn: position.column,
				},
				text,
			},
		]);
	}
	/**
	 * Selects a range of text in the editor.
	 *
	 * @param {UmbCodeEditorRange} range
	 * @memberof UmbCodeEditor
	 */
	select(range: UmbCodeEditorRange) {
		if (!this.#editor) throw new Error('Editor object not found');
		this.#editor.setSelection(range);
	}
	/**
	 * Changes the theme of the editor.
	 *
	 * @template T
	 * @param {(CodeEditorTheme | T)} theme
	 * @memberof UmbCodeEditor
	 */
	setTheme<T extends string>(theme: CodeEditorTheme | T) {
		if (!this.#editor) throw new Error('Editor object not found');
		monaco.editor.setTheme(theme);
	}
	/**
	 * Runs callback on change of model content. (for example when typing)
	 *
	 * @param {() => void} callback
	 * @memberof UmbCodeEditor
	 */
	onChangeModelContent(callback: () => void) {
		if (!this.#editor) throw new Error('Editor object not found');
		this.#editor.onDidChangeModelContent(() => {
			callback();
		});
	}
	/**
	 * Runs callback on change of model (when the entire model is replaced	)
	 *
	 * @param {() => void} callback
	 * @memberof UmbCodeEditor
	 */
	onDidChangeModel(callback: () => void) {
		if (!this.#editor) throw new Error('Editor object not found');
		this.#editor.onDidChangeModel(() => {
			callback();
		});
	}
	/**
	 * Runs callback on change of cursor position. Gives as parameter the new position.
	 *
	 * @param {((e: UmbCodeEditorCursorPositionChangedEvent | undefined) => void)} callback
	 * @memberof UmbCodeEditor
	 */
	onDidChangeCursorPosition(callback: (e: UmbCodeEditorCursorPositionChangedEvent | undefined) => void) {
		if (!this.#editor) throw new Error('Editor object not found');
		this.#editor.onDidChangeCursorPosition((event) => {
			callback(event);
		});
	}
	/**
	 * Runs callback on change of cursor selection. Gives as parameter the new selection.
	 *
	 * @param {((e: UmbCodeEditorCursorSelectionChangedEvent | undefined) => void)} callback
	 * @memberof UmbCodeEditor
	 */
	onDidChangeCursorSelection(callback: (e: UmbCodeEditorCursorSelectionChangedEvent | undefined) => void) {
		if (!this.#editor) throw new Error('Editor object not found');
		this.#editor.onDidChangeCursorSelection((event) => {
			callback(event);
		});
	}
}
