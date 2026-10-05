import { UMB_SCRIPT_WORKSPACE_CONTEXT } from '../../script-workspace.context-token.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

import '@umbraco-cms/backoffice/code-editor';

@customElement('umb-script-code-editor-workspace-view')
export class UmbScriptCodeEditorWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _content?: string | null = '';

	#context?: typeof UMB_SCRIPT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_SCRIPT_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context?.content, (content) => (this._content = content));
		});
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#context?.setContent(value);
	}

	override render() {
		return html`
			<uui-box>
				<!-- the div below in the header is to make the box display nicely with code editor -->
				<div slot="header"></div>
				${this.#renderCodeEditor()}
			</uui-box>
		`;
	}

	#renderCodeEditor() {
		if (this._content === undefined) {
			return nothing;
		}

		return html`
			<umb-code-editor
				id="content"
				language="javascript"
				.code=${this._content ?? ''}
				@input=${this.#onCodeEditorInput}></umb-code-editor>
		`;
	}

	static override styles = [
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
		`,
	];
}

export default UmbScriptCodeEditorWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-script-code-editor-workspace-view': UmbScriptCodeEditorWorkspaceViewElement;
	}
}
