import type { UmbCodeEditorController } from './code-editor.controller.js';
import type { CodeEditorLanguage, CodeEditorSearchOptions, UmbCodeEditorHost } from './code-editor.model.js';
import { CodeEditorTheme } from './code-editor.model.js';
import { UmbCodeEditorLoadedEvent } from './code-editor-loaded.event.js';
import { UMB_THEME_CONTEXT } from '@umbraco-cms/backoffice/themes';
import type { PropertyValues, Ref } from '@umbraco-cms/backoffice/external/lit';
import {
	createRef,
	css,
	customElement,
	html,
	property,
	ref,
	state,
	unsafeCSS,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-code-editor';

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
 * @fires loaded - Fired when the editor is loaded and ready to use.
 */
@customElement(elementName)
export class UmbCodeEditorElement extends UmbLitElement implements UmbCodeEditorHost {
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

	@state()
	private _loading = true;

	@state()
	private _styles?: string;

	constructor() {
		super();

		this.consumeContext(UMB_THEME_CONTEXT, (instance) => {
			this.observe(
				instance.theme,
				(themeAlias) => {
					this.theme = themeAlias ? this.#translateTheme(themeAlias) : CodeEditorTheme.Light;
				},
				'_observeTheme',
			);
		});
	}

	override async firstUpdated() {
		const { styles } = await import('@umbraco-cms/backoffice/external/monaco-editor');
		this._styles = styles;

		const controller = (await import('./code-editor.controller.js')).default;
		this.#editor = new controller(this);

		this._loading = false;
		this.dispatchEvent(new UmbCodeEditorLoadedEvent());
	}

	protected override updated(_changedProperties: PropertyValues<this>): void {
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
	find(text: string, searchOptions: CodeEditorSearchOptions = <CodeEditorSearchOptions>{}) {
		return this.#editor?.find(text, searchOptions);
	}

	override render() {
		return html`
			${this.#renderStyles()}
			${when(this._loading, () => html`<div id="loader-container"><uui-loader></uui-loader></div>`)}
			<div id="editor-container" ${ref(this.containerRef)}></div>
		`;
	}

	#renderStyles() {
		if (!this._styles) return;
		return html`
			<style>
				${unsafeCSS(this._styles)}
			</style>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
			}

			#loader-container {
				display: grid;
				place-items: center;
				min-height: calc(100dvh - 260px);
			}

			#editor-container {
				width: var(--editor-width);
				height: var(--editor-height, 100%);

				--vscode-scrollbar-shadow: #dddddd;
				--vscode-scrollbarSlider-background: var(--uui-color-disabled-contrast);
				--vscode-scrollbarSlider-hoverBackground: rgba(100, 100, 100, 0.7);
				--vscode-scrollbarSlider-activeBackground: rgba(0, 0, 0, 0.6);

				/* a hacky workaround this issue: https://github.com/microsoft/monaco-editor/issues/3217
			   should probably be removed when the issue is fixed */
				.view-lines {
					font-feature-settings: revert !important;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCodeEditorElement;
	}
}
