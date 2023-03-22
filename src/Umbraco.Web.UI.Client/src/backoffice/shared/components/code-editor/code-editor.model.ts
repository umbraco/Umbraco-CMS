export type CodeEditorLanguage = 'razor' | 'typescript' | 'javascript' | 'css' | 'markdown' | 'json' | 'html';

export enum CodeEditorTheme {
	Light = 'umb-light',
	Dark = 'umb-dark',
	HighContrastLight = 'umb-hc-light',
	HighContrastDark = 'umb-hc-dark',
}

export interface UmbCodeEditorHost extends HTMLElement {
	container: HTMLElement;
	language: CodeEditorLanguage;
	theme: CodeEditorTheme;
	code: string;
	readonly: boolean;
	label: string;
}

export interface UmbCodeEditorCursorPosition {
	column: number;
	lineNumber: number;
}

export interface UmbCodeEditorRange {
	startLineNumber: number;
	startColumn: number;
	endLineNumber: number;
	endColumn: number;
}

export interface UmbCodeEditorSelection {
	startLineNumber: number;
	startColumn: number;
	endLineNumber: number;
	endColumn: number;
	positionColumn: number;
	positionLineNumber: number;
	selectionStartColumn: number;
	selectionStartLineNumber: number;
}

export interface UmbCodeEditorCursorPositionChangedEvent {
	position: UmbCodeEditorCursorPosition;
	secondaryPositions: UmbCodeEditorCursorPosition[];
}

export interface UmbCodeEditorCursorSelectionChangedEvent {
	selection: UmbCodeEditorSelection;
	secondarySelections: UmbCodeEditorSelection[];
}

export interface CodeEditorConstructorOptions {
	/**
	 * The initial value of the auto created model in the editor.
	 */
	value?: string;
	/**
	 * The initial language of the auto created model in the editor.
	 */
	language?: CodeEditorLanguage;
	/**
	 * Initial theme to be used for rendering.
	 * The current out-of-the-box available themes are: 'vs' (default), 'vs-dark', 'hc-black', 'hc-light.
	 * You can create custom themes via `monaco.editor.defineTheme`.
	 * To switch a theme, use `monaco.editor.setTheme`.
	 * **NOTE**: The theme might be overwritten if the OS is in high contrast mode, unless `autoDetectHighContrast` is set to false.
	 */
	theme?: CodeEditorTheme;
	/**
	 * Container element to use for ARIA messages.
	 * Defaults to document.body.
	 */
	ariaContainerElement?: HTMLElement;
	/**
	 * The aria label for the editor's textarea (when it is focused).
	 */
	ariaLabel?: string;
	/**
	 * The `tabindex` property of the editor's textarea
	 */
	tabIndex?: number;

	/**
	 * Control the rendering of line numbers.
	 * Defaults to `true`.
	 */
	lineNumbers?: boolean;
	/**
	 * Class name to be added to the editor.
	 */
	extraEditorClassName?: string;
	/**
	 * Should the editor be read only. See also `domReadOnly`.
	 * Defaults to false.
	 */
	readOnly?: boolean;
	/**
	 * Control the behavior and rendering of the minimap.
	 */
	minimap?: boolean;
	/**
	 * Enable that the editor will install a ResizeObserver to check if its container dom node size has changed.
	 * Defaults to false.
	 */
	automaticLayout?: boolean;
	/**
	 * Control the wrapping of the editor.
	 * When `wordWrap` = "off", the lines will never wrap.
	 * When `wordWrap` = "on", the lines will wrap at the viewport width.
	 * When `wordWrap` = "wordWrapColumn", the lines will wrap at `wordWrapColumn`.
	 * When `wordWrap` = "bounded", the lines will wrap at min(viewport width, wordWrapColumn).
	 * Defaults to "off".
	 */
	wordWrap?: 'off' | 'on' | 'wordWrapColumn' | 'bounded';
	/**
	 * Enable detecting links and making them clickable.
	 * Defaults to true.
	 */
	links?: boolean;
	/**
	 * Enable inline color decorators and color picker rendering.
	 */
	colorDecorators?: boolean;
	/**
	 * Controls the max number of color decorators that can be rendered in an editor at once.
	 */
	colorDecoratorsLimit?: number;
	/**
	 * Enable custom contextmenu.
	 * Defaults to true.
	 */
	contextmenu?: boolean;
	/**
	 * The modifier to be used to add multiple cursors with the mouse.
	 * Defaults to 'alt'
	 */
	multiCursorModifier?: 'ctrlCmd' | 'alt';
	/**
	 * Controls the max number of text cursors that can be in an active editor at once.
	 */
	multiCursorLimit?: number;
	/**
	 * Controls the number of lines in the editor that can be read out by a screen reader
	 */
	accessibilityPageSize?: number;
	/**
	 * Controls the spacing around the editor.
	 * @type {{bottom: number; top: number}}
	 * @memberof CodeEditorConstructorOptions
	 */
	padding?: { bottom: number; top: number };
	/**
	 * Controls if the editor should allow to move selections via drag and drop.
	 * Defaults to false.
	 */
	dragAndDrop?: boolean;
	/**
	 * Show code lens
	 * Defaults to true.
	 */
	codeLens?: boolean;
	/**
	 * Control the behavior and rendering of the code action lightbulb.
	 */
	lightbulb?: boolean;
	/**
	 * Enable code folding.
	 * Defaults to true.
	 */
	folding?: boolean;
	/**
	 * The font family
	 */
	fontFamily?: string;
	/**
	 * The font weight
	 */
	fontWeight?: string;
	/**
	 * The font size
	 */
	fontSize?: number;
	/**
	 * The line height
	 */
	lineHeight?: number;
	/**
	 * The letter spacing
	 */
	letterSpacing?: number;
}

export interface CodeEditorSearchOptions {
	/**
	 *   Limit the searching to only search inside the editable range of the model.
	 *
	 * @type {boolean}
	 * @memberof CodeEditorSearchOptions
	 */
	searchOnlyEditableRange: boolean;
	/**
	 * Used to indicate that searchString is a regular expression.
	 *
	 * @type {boolean}
	 * @memberof CodeEditorSearchOptions
	 */
	isRegex: boolean;
	/**
	 * Force the matching to match lower/upper case exactly.
	 *
	 * @type {boolean}
	 * @memberof CodeEditorSearchOptions
	 */
	matchCase: boolean;
	/**
	 * Force the matching to match entire words only. Pass null otherwise.
	 *
	 * @type {string}
	 * @memberof CodeEditorSearchOptions
	 */
	wordSeparators: string | null;
	/**
	 * The result will contain the captured groups.
	 *
	 * @type {boolean}
	 * @memberof CodeEditorSearchOptions
	 */
	captureMatches: boolean;
}
