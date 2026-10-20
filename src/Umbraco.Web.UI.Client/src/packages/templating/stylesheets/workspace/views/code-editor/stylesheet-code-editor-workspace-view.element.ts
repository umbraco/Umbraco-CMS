import { UMB_STYLESHEET_ENTITY_TYPE } from '../../../entity.js';
import type { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { UMB_STYLESHEET_WORKSPACE_CONTEXT } from '../../stylesheet-workspace.context-token.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from '@umbraco-cms/backoffice/schema-lockdown';

import '@umbraco-cms/backoffice/code-editor';

@customElement('umb-stylesheet-code-editor-workspace-view')
export class UmbStylesheetCodeEditorWorkspaceViewElement extends UmbLitElement {
	@state()
	private _content?: string | null = '';

	// Restricted until the schema lockdown matrix confirms the operation is allowed (safe default).
	@state()
	private _isRestricted = true;

	#stylesheetWorkspaceContext?: UmbStylesheetWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_SCHEMA_LOCKDOWN_CONTEXT, (context) => {
			this.observe(context?.state, () => {
				this._isRestricted = context?.isAllowed(UMB_STYLESHEET_ENTITY_TYPE, 'update') !== true;
			});
		});

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
		return html`${this.#renderSchemaLockdownNotice()}
			<uui-box>
				<div slot="header" id="code-editor-menu-container"></div>
				${this.#renderCodeEditor()}
			</uui-box>`;
	}

	#renderSchemaLockdownNotice() {
		if (!this._isRestricted) return nothing;
		return html`
			<uui-box id="schema-lockdown-notice">
				<div class="notice">
					<umb-icon name="icon-info"></umb-icon>
					<div>
						<strong><umb-localize key="schemaLockdown_headline">Schema Locked</umb-localize></strong>
						<p><umb-localize key="schemaLockdown_notice"></umb-localize></p>
					</div>
				</div>
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

			#schema-lockdown-notice {
				display: block;
				min-height: 0;
				margin: var(--uui-size-layout-1) var(--uui-size-layout-1) 0;
				--uui-box-default-padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border-left: 4px solid var(--uui-color-warning-standalone, #f0ac00);
			}

			#schema-lockdown-notice .notice {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: flex-start;
			}

			#schema-lockdown-notice umb-icon {
				flex: 0 0 auto;
				font-size: var(--uui-size-6);
				margin-top: 2px;
				color: var(--uui-color-warning-standalone, #f0ac00);
			}

			#schema-lockdown-notice p {
				margin: var(--uui-size-space-2) 0 0;
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

export default UmbStylesheetCodeEditorWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-code-editor-workspace-view': UmbStylesheetCodeEditorWorkspaceViewElement;
	}
}
