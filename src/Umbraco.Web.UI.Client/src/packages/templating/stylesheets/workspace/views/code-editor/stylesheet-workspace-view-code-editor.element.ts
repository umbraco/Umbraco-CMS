import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UMB_STYLESHEET_WORKSPACE_CONTEXT, UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';

@customElement('umb-stylesheet-workspace-view-code-editor')
export class UmbStylesheetWorkspaceViewCodeEditorElement extends UmbLitElement {
	@state()
	private _content?: string | null = '';

	@state()
	private _ready?: boolean = false;

	#stylesheetWorkspaceContext?: UmbStylesheetWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_STYLESHEET_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#stylesheetWorkspaceContext = workspaceContext;

			this.observe(this.#stylesheetWorkspaceContext.content, (content) => {
				this._content = content;
			});

			this.observe(this.#stylesheetWorkspaceContext.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#stylesheetWorkspaceContext?.sendContentGetRules();
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#stylesheetWorkspaceContext?.setContent(value);
	}

	#renderCodeEditor() {
		return html`<umb-code-editor
			language="css"
			id="content"
			.code=${this._content ?? ''}
			@input=${this.#onCodeEditorInput}></umb-code-editor>`;
	}

	render() {
		return html` <uui-box>
			<div slot="header" id="code-editor-menu-container"></div>
			${this._ready
				? this.#renderCodeEditor()
				: html`<div id="loader-container">
						<uui-loader></uui-loader>
				  </div>`}
		</uui-box>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
			}

			#loader-container {
				display: grid;
				place-items: center;
				min-height: calc(100dvh - 300px);
			}

			umb-code-editor {
				--editor-height: calc(100dvh - 300px);
			}

			uui-box {
				min-height: calc(100dvh - 360px);
				margin: var(--uui-size-layout-1);
				--uui-box-default-padding: 0;
				/* remove header border bottom as code editor looks better in this box */
				--uui-color-divider-standalone: transparent;
			}

			#workspace-header {
				width: 100%;
			}
		`,
	];
}

export default UmbStylesheetWorkspaceViewCodeEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace-view-code-editor': UmbStylesheetWorkspaceViewCodeEditorElement;
	}
}
