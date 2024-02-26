import type { UmbScriptWorkspaceContext } from './script-workspace.context.js';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-script-workspace-editor')
export class UmbScriptWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string = '';

	@state()
	private _content?: string | null = '';

	@state()
	private _path?: string | null = '';

	@state()
	private _ready?: boolean = false;

	@state()
	private _isNew?: boolean = false;

	#scriptsWorkspaceContext?: UmbScriptWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#scriptsWorkspaceContext = workspaceContext as UmbScriptWorkspaceContext;

			this.observe(this.#scriptsWorkspaceContext.name, (name) => {
				this._name = name;
			});

			this.observe(this.#scriptsWorkspaceContext.content, (content) => {
				this._content = content;
			});

			this.observe(this.#scriptsWorkspaceContext.path, (path) => {
				this._path = path;
			});

			this.observe(this.#scriptsWorkspaceContext.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});

			this.observe(this.#scriptsWorkspaceContext.isNew, (isNew) => {
				this._isNew = isNew;
			});
		});
	}

	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#scriptsWorkspaceContext?.setName(value);
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#scriptsWorkspaceContext?.setContent(value);
	}

	#renderCodeEditor() {
		return html`<umb-code-editor
			language="javascript"
			id="content"
			.code=${this._content ?? ''}
			@input=${this.#onCodeEditorInput}></umb-code-editor>`;
	}

	render() {
		return html`<umb-workspace-editor alias="Umb.Workspace.Script">
			<div id="workspace-header" slot="header">
				<uui-input
					placeholder="Enter name..."
					.value=${this._name}
					@input=${this.#onNameInput}
					label="Script name"
					?readonly=${this._isNew === false}></uui-input>
				<small>/scripts${this._path}</small>
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
		</umb-workspace-editor>`;
	}

	static styles = [
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
