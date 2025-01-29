import { UMB_SCRIPT_WORKSPACE_CONTEXT } from './script-workspace.context-token.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';

import '@umbraco-cms/backoffice/code-editor';

@customElement('umb-script-workspace-editor')
export class UmbScriptWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _content?: string | null = '';

	@state()
	private _isNew?: boolean;

	#context?: typeof UMB_SCRIPT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_SCRIPT_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context.content, (content) => (this._content = content));
			this.observe(this.#context.isNew, (isNew) => (this._isNew = isNew));
		});
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#context?.setContent(value);
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-workspace-header-name-editable
					slot="header"
					?readonly=${this._isNew === false}></umb-workspace-header-name-editable>
				${this.#renderBody()}
			</umb-entity-detail-workspace-editor>
		`;
	}

	#renderBody() {
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

export default UmbScriptWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-script-workspace-editor': UmbScriptWorkspaceEditorElement;
	}
}
