import type { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { UMB_STYLESHEET_WORKSPACE_CONTEXT } from '../../stylesheet-workspace.context-token.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '@umbraco-cms/backoffice/code-editor';

@customElement('umb-stylesheet-code-editor-workspace-view')
export class UmbStylesheetCodeEditorWorkspaceViewElement extends UmbLitElement {
	@state()
	private _content?: string | null = '';

	#stylesheetWorkspaceContext?: UmbStylesheetWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_STYLESHEET_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#stylesheetWorkspaceContext = workspaceContext;

			this.observe(this.#stylesheetWorkspaceContext?.content, (content) => {
				this._content = content;
			});
		});
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#stylesheetWorkspaceContext?.setContent(value);
	}

	override render() {
		return html` <uui-box>
			<div slot="header" id="code-editor-menu-container"></div>
			${this.#renderCodeEditor()}
		</uui-box>`;
	}

	#renderCodeEditor() {
		if (this._content === undefined) {
			return nothing;
		}

		return html`
			<umb-code-editor
				id="content"
				language="css"
				.code=${this._content ?? ''}
				@input=${this.#onCodeEditorInput}></umb-code-editor>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
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
		`,
	];
}

export default UmbStylesheetCodeEditorWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-code-editor-workspace-view': UmbStylesheetCodeEditorWorkspaceViewElement;
	}
}
