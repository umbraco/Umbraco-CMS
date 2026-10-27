import { getQuerySnippet } from '../../../../utils/index.js';
import type { UmbTemplatingInsertMenuElement } from '../../../../local-components/insert-menu/index.js';
import { UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT } from '../../partial-view-workspace.context-token.js';
import { css, customElement, html, nothing, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_TEMPLATE_QUERY_BUILDER_MODAL } from '@umbraco-cms/backoffice/template';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

import '@umbraco-cms/backoffice/code-editor';
import '../../../../local-components/insert-menu/index.js';

@customElement('umb-partial-view-code-editor-workspace-view')
export class UmbPartialViewCodeEditorWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _content?: string | null = '';

	@state()
	private _isReadOnly = false;

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#workspaceContext?: typeof UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.observe(context?.readOnly, (isReadOnly) => (this._isReadOnly = isReadOnly ?? false), '_observeIsReadOnly');
		});

		this.consumeContext(UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;

			this.observe(this.#workspaceContext?.content, (content) => {
				this._content = content;
			});
		});
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#workspaceContext?.setContent(value);
	}

	#insertSnippet(event: Event) {
		const target = event.target as UmbTemplatingInsertMenuElement;
		const value = target.value as string;
		this._codeEditor?.insert(value);
	}

	async #openQueryBuilder() {
		const queryBuilderModalValue = await umbOpenModal(this, UMB_TEMPLATE_QUERY_BUILDER_MODAL).catch(() => undefined);

		if (queryBuilderModalValue?.value) {
			this._codeEditor?.insert(getQuerySnippet(queryBuilderModalValue.value));
		}
	}

	override render() {
		return html`
			<uui-box>
				<div slot="header" id="code-editor-menu-container">
					<umb-templating-insert-menu
						@insert=${this.#insertSnippet}
						?disabled=${this._isReadOnly}
						hidePartialViews></umb-templating-insert-menu>
					<uui-button
						look="secondary"
						id="query-builder-button"
						label=${this.localize.term('template_queryBuilder')}
						?disabled=${this._isReadOnly}
						@click=${this.#openQueryBuilder}>
						<uui-icon name="icon-wand"></uui-icon>
						<umb-localize key="template_queryBuilder">Query builder</umb-localize>
					</uui-button>
				</div>
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
				language="razor"
				.code=${this._content ?? ''}
				?readonly=${this._isReadOnly}
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
				--editor-height: calc(100dvh - 300px);
			}

			uui-box {
				min-height: calc(100dvh - 300px);
				margin: var(--uui-size-layout-1);
				--uui-box-default-padding: 0;
				/* remove header border bottom as code editor looks better in this box */
				--uui-color-divider-standalone: transparent;
			}

			#code-editor-menu-container uui-icon:not([name='icon-delete']) {
				margin-right: var(--uui-size-space-3);
			}

			#code-editor-menu-container {
				display: flex;
				justify-content: flex-end;
				gap: var(--uui-size-space-3);
				width: 100%;
			}
		`,
	];
}

export default UmbPartialViewCodeEditorWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-code-editor-workspace-view': UmbPartialViewCodeEditorWorkspaceViewElement;
	}
}
