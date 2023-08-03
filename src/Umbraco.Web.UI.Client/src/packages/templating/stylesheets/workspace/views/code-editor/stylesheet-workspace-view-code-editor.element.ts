import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';

@customElement('umb-stylesheet-workspace-view-code-editor')
export class UmbStylesheetWorkspaceViewCodeEditorElement extends UmbLitElement {
	@state()
	private _content?: string | null = '';

	@state()
	private _path?: string | null = '';

	@state()
	private _ready?: boolean = false;

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#stylesheetWorkspaceContext?: UmbStylesheetWorkspaceContext;
	private _modalContext?: UmbModalManagerContext;

	#isNew = false;

	constructor() {
		super();

		//tODO: should this be called something else here?
		this.consumeContext('UmbEntityWorkspaceContext', (workspaceContext: UmbStylesheetWorkspaceContext) => {
			this.#stylesheetWorkspaceContext = workspaceContext;

			this.observe(this.#stylesheetWorkspaceContext.content, (content) => {
				this._content = content;
			});

			this.observe(this.#stylesheetWorkspaceContext.path, (path) => {
				this._path = path;
			});

			this.observe(this.#stylesheetWorkspaceContext.isNew, (isNew) => {
				this.#isNew = !!isNew;
			});

			this.observe(this.#stylesheetWorkspaceContext.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});
		});
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
