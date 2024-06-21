import { UMB_SCRIPT_WORKSPACE_CONTEXT } from './script-workspace.context-token.js';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-script-workspace-editor')
export class UmbScriptWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string = '';

	@state()
	private _content?: string | null = '';

	@state()
	private _ready?: boolean = false;

	@state()
	private _isNew?: boolean;

	#context?: typeof UMB_SCRIPT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_SCRIPT_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;

			this.observe(this.#context.name, (name) => {
				this._name = name;
			});

			this.observe(this.#context.content, (content) => {
				this._content = content;
			});

			this.observe(this.#context.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});

			this.observe(this.#context.isNew, (isNew) => {
				this._isNew = isNew;
			});
		});
	}

	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#context?.setName(value);
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#context?.setContent(value);
	}

	#renderCodeEditor() {
		return html`<umb-code-editor
			language="javascript"
			id="content"
			.code=${this._content ?? ''}
			@input=${this.#onCodeEditorInput}></umb-code-editor>`;
	}

	override render() {
		return this._isNew !== undefined
			? html`<umb-workspace-editor alias="Umb.Workspace.Script">
					<div id="workspace-header" slot="header">
						<uui-input
							placeholder="Enter name..."
							.value=${this._name}
							@input=${this.#onNameInput}
							label="Script name"
							?readonly=${this._isNew === false}
							${umbFocus()}></uui-input>
					</div>
					<uui-box>
						<!-- the div below in the header is to make the box display nicely with code editor -->
						<div slot="header"></div>
						${this._ready
							? this.#renderCodeEditor()
							: html`<div id="loader-container">
									<uui-loader></uui-loader>
								</div>`}
					</uui-box>
				</umb-workspace-editor>`
			: nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
			}

			#loader-container {
				display: grid;
				place-items: center;
				min-height: calc(100dvh - 260px);
			}

			umb-code-editor {
				--editor-height: calc(100dvh - 260px);
			}

			uui-box {
				min-height: calc(100dvh - 260px);
				margin: var(--uui-size-layout-1);
				--uui-box-default-padding: 0;
				/* remove header border bottom as code editor looks better in this box */
				--uui-color-divider-standalone: transparent;
			}

			#workspace-header {
				width: 100%;
			}

			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbScriptWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-script-workspace-editor': UmbScriptWorkspaceEditorElement;
	}
}
