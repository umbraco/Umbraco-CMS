import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import { UMB_STYLESHEET_WORKSPACE_CONTEXT } from './stylesheet-workspace.context-token.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from '@umbraco-cms/backoffice/schema-lockdown';

@customElement('umb-stylesheet-workspace-editor')
export class UmbStylesheetWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _isNew?: boolean;

	// Restricted until the schema lockdown matrix confirms the operation is allowed (safe default).
	@state()
	private _isRestricted = true;

	#context?: typeof UMB_STYLESHEET_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_SCHEMA_LOCKDOWN_CONTEXT, (context) => {
			this.observe(context?.state, () => {
				this._isRestricted = context?.isAllowed(UMB_STYLESHEET_ENTITY_TYPE, 'update') !== true;
			});
		});

		this.consumeContext(UMB_STYLESHEET_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context?.isNew, (isNew) => (this._isNew = isNew));
		});
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

	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-workspace-header-name-editable
					slot="header"
					?readonly=${this._isNew === false}></umb-workspace-header-name-editable>
				${this.#renderSchemaLockdownNotice()}
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
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

export default UmbStylesheetWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace-editor': UmbStylesheetWorkspaceEditorElement;
	}
}
