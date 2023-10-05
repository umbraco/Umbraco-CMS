import { UmbCodeEditorElement, loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { css, html, customElement, query, property } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-input-markdown
 * @fires change - when the value of the input changes
 */
@customElement('umb-input-markdown')
export class UmbInputMarkdownElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@property({ type: Boolean })
	preview?: boolean;

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	@query('umb-code-editor')
	_codeEditor?: UmbCodeEditorElement;

	constructor() {
		super();
		this.#loadCodeEditor();
	}

	async #loadCodeEditor() {
		try {
			await loadCodeEditor();
			this.#isCodeEditorReady.next(true);
		} catch (error) {
			console.error(error);
		}
	}

	render() {
		return html` <div id="actions"></div>
			<umb-code-editor language="markdown" .code=${this.value as string}></umb-code-editor>
			${this.renderPreview()}`;
	}

	renderPreview() {
		if (!this.preview) return;
		return html`<div>TODO Preview</div>`;
	}

	static styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
			#actions {
				background-color: var(--uui-color-background-alt);
				display: flex;
			}

			umb-code-editor {
				height: 200px;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-divider-emphasis);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-markdown': UmbInputMarkdownElement;
	}
}
