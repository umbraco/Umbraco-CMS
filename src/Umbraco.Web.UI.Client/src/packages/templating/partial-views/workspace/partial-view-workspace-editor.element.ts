import type { UmbTemplatingInsertMenuElement } from '../../components/index.js';
import { getQuerySnippet } from '../../utils/index.js';
import { UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT } from './partial-view-workspace.context.js';
import { UMB_TEMPLATE_QUERY_BUILDER_MODAL } from '@umbraco-cms/backoffice/template';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

@customElement('umb-partial-view-workspace-editor')
export class UmbPartialViewWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string = '';

	@state()
	private _content?: string | null = '';

	@state()
	private _path?: string | null = '';

	@state()
	private _ready: boolean = false;

	@state()
	private _isNew?: boolean = false;

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#workspaceContext?: typeof UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.observe(this.#workspaceContext.name, (name) => {
				this._name = name;
			});

			this.observe(this.#workspaceContext.content, (content) => {
				this._content = content;
			});

			this.observe(this.#workspaceContext.path, (path) => {
				this._path = path;
			});

			this.observe(this.#workspaceContext.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});

			this.observe(this.#workspaceContext.isNew, (isNew) => {
				this._isNew = isNew;
			});
		});
	}

	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#workspaceContext?.setName(value);
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
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const queryBuilderModal = modalManager.open(this, UMB_TEMPLATE_QUERY_BUILDER_MODAL);

		queryBuilderModal?.onSubmit().then((queryBuilderModalValue) => {
			if (queryBuilderModalValue.value) this._codeEditor?.insert(getQuerySnippet(queryBuilderModalValue.value));
		});
	}

	#renderCodeEditor() {
		return html`<umb-code-editor
			language="razor"
			id="content"
			.code=${this._content ?? ''}
			@input=${this.#onCodeEditorInput}></umb-code-editor>`;
	}

	render() {
		return html`<umb-workspace-editor alias="Umb.Workspace.PartialView">
			<div id="workspace-header" slot="header">
				<uui-input
					placeholder="Enter name..."
					.value=${this._name}
					@input=${this.#onNameInput}
					label="Partial view name"
					?readonly=${this._isNew === false}></uui-input>
				<small>Views/Partials${this._path}</small>
			</div>
			<uui-box>
				<div slot="header" id="code-editor-menu-container">
					<umb-templating-insert-menu @insert=${this.#insertSnippet}></umb-templating-insert-menu>
					<uui-button look="secondary" id="query-builder-button" label="Query builder" @click=${this.#openQueryBuilder}>
						<uui-icon name="icon-wand"></uui-icon>Query builder
					</uui-button>
				</div>
				${this._ready
					? this.#renderCodeEditor()
					: html`<div id="loader-container">
							<uui-loader></uui-loader>
					  </div>`}
			</uui-box>
		</umb-workspace-editor>`;
	}

	static styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#loader-container {
				display: grid;
				place-items: center;
				min-height: calc(100dvh - 360px);
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

			#workspace-header {
				width: 100%;
			}

			uui-input {
				width: 100%;
			}

			#code-editor-menu-container uui-icon:not([name='icon-delete']) {
				margin-right: var(--uui-size-space-3);
			}

			#insert-menu {
				margin: 0;
				padding: 0;
				margin-top: var(--uui-size-space-3);
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
				min-width: calc(100% + var(--uui-size-8, 24px));
			}

			#insert-menu > li,
			ul {
				padding: 0;
				width: 100%;
				list-style: none;
			}

			.insert-menu-item {
				width: 100%;
			}

			#code-editor-menu-container {
				display: flex;
				justify-content: flex-end;
				gap: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbPartialViewWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-workspace-editor': UmbPartialViewWorkspaceEditorElement;
	}
}
