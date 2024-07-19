import { UMB_SCRIPT_WORKSPACE_CONTEXT } from './script-workspace.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

import '@umbraco-cms/backoffice/code-editor';

@customElement('umb-script-workspace-editor')
export class UmbScriptWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string = '';

	@state()
	private _content?: string | null = '';

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

	override render() {
		if (this._isNew === undefined) return;
		return html`
			<umb-workspace-editor alias="Umb.Workspace.Script">
				<div id="workspace-header" slot="header">
					<uui-input
						placeholder=${this.localize.term('placeholders_entername')}
						.value=${this._name}
						@input=${this.#onNameInput}
						label=${this.localize.term('placeholders_entername')}
						?readonly=${this._isNew === false}
						${umbFocus()}>
					</uui-input>
				</div>
				<uui-box>
					<!-- the div below in the header is to make the box display nicely with code editor -->
					<div slot="header"></div>
					${this.#renderCodeEditor()}
				</uui-box>
			</umb-workspace-editor>
		`;
	}

	#renderCodeEditor() {
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
