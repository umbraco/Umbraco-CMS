import { css, html, PropertyValues } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { createRef, Ref, ref } from 'lit/directives/ref.js';
import { UMB_THEME_CONTEXT_TOKEN } from '../../../themes/theme.context';
import { UmbCodeEditorController } from './code-editor.controller';
import { CodeEditorLanguage, CodeEditorTheme, UmbCodeEditorHost } from './code-editor.model';
import { monacoEditorStyles, monacoJumpingCursorHack } from './styles';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
/**
 * A custom element that renders a code editor. Code editor is based on the Monaco Editor library.
 * The element will listen to the theme context and update the theme accordingly.
 * Parts of the monaco Api is exposed through the `editor` property. You can access the monaco editor instance through `editor.monacoEditor`.
 *
 * @element umb-code-editor
 *
 * @export
 * @class UmbCodeEditorElement
 * @extends {UmbLitElement}
 * @implements {UmbCodeEditorHost}
 * @fires input - Fired when the value of the editor changes.
 * @fires change - Fired when the entire model of editor is replaced.
 */
@customElement('umb-code-editor')
export class UmbCodeEditorElement extends UmbLitElement implements UmbCodeEditorHost {
	static styles = [
		monacoEditorStyles,
		monacoJumpingCursorHack,
		css`
			:host {
				display: block;
			}
			#editor-container {
				width: var(--editor-width);
				height: var(--editor-height, 100%);

				--vscode-scrollbar-shadow: #dddddd;
				--vscode-scrollbarSlider-background: var(--uui-color-disabled-contrast);
				--vscode-scrollbarSlider-hoverBackground: rgba(100, 100, 100, 0.7);
				--vscode-scrollbarSlider-activeBackground: rgba(0, 0, 0, 0.6);
			}
		`,
	];

	private containerRef: Ref<HTMLElement> = createRef();

	get container() {
		if (!this.containerRef?.value) throw new Error('Container not found');
		return this.containerRef!.value;
	}

	#editor?: UmbCodeEditorController;

	get editor() {
		return this.#editor;
	}
	/**
	 * Theme of the editor. Default is light. Element will listen to the theme context and update the theme accordingly.
	 *
	 * @type {CodeEditorTheme}
	 * @memberof UmbCodeEditorElement
	 */
	@property()
	theme: CodeEditorTheme = CodeEditorTheme.Light;
	/**
	 * Language of the editor. Default is javascript.
	 *
	 * @type {CodeEditorLanguage}
	 * @memberof UmbCodeEditorElement
	 */
	@property()
	language: CodeEditorLanguage = 'javascript';
	/**
	 * Label of the editor. Default is 'Code Editor'.
	 *
	 * @memberof UmbCodeEditorElement
	 */
	@property()
	label = 'Code Editor';

	//TODO - this should be called a value
	#code = '';
	/**
	 * Value of the editor. Default is empty string.
	 *
	 * @readonly
	 * @memberof UmbCodeEditorElement
	 */
	@property()
	get code() {
		return this.#code;
	}

	set code(value: string) {
		const oldValue = this.#code;
		this.#code = value;
		if (this.#editor) {
			this.#editor.value = value;
		}
		this.requestUpdate('code', oldValue);
	}
	/**
	 * Whether the editor is readonly. Default is false.
	 *
	 * @memberof UmbCodeEditorElement
	 */
	@property({ type: Boolean, attribute: 'readonly' })
	readonly = false;

	constructor() {
		super();
		this.consumeContext(UMB_THEME_CONTEXT_TOKEN, (instance) => {
			instance.theme.subscribe((themeAlias) => {
				this.theme = themeAlias ? this.#translateTheme(themeAlias) : CodeEditorTheme.Light;
			});
		});
	}

	firstUpdated() {
		this.#editor = new UmbCodeEditorController(this);
	}

	protected updated(_changedProperties: PropertyValues<this>): void {
		if (_changedProperties.has('theme') || _changedProperties.has('language')) {
			this.#editor?.updateOptions({
				theme: this.theme,
				language: this.language,
			});
		}
	}

	#translateTheme(theme: string) {
		switch (theme) {
			case 'umb-light-theme':
				return CodeEditorTheme.Light;
			case 'umb-dark-theme':
				return CodeEditorTheme.Dark;
			case 'umb-high-contrast-theme':
				return CodeEditorTheme.HighContrastLight;
			default:
				return CodeEditorTheme.Light;
		}
	}
	/**
	 * Inserts text at the current cursor position.
	 *
	 * @param {string} text
	 * @memberof UmbCodeEditorElement
	 */
	insert(text: string) {
		this.#editor?.insert(text);
	}
	/**
	 * Finds all occurrence of the given string or matches the given regular expression.
	 *
	 * @param {string} text
	 * @return {*}
	 * @memberof UmbCodeEditorElement
	 */
	find(text: string) {
		return this.#editor?.find(text);
	}

	render() {
		return html` <div id="editor-container" ${ref(this.containerRef)}></div> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-editor': UmbCodeEditorElement;
	}
}
