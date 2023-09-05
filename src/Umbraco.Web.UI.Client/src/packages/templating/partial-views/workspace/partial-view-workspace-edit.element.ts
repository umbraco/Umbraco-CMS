import { UmbTemplatingInsertMenuElement } from '../../components/index.js';
import { UMB_TEMPLATE_QUERY_BUILDER_MODAL } from '../../templates/modals/modal-tokens.js';
import { getQuerySnippet } from '../../utils.js';
import { UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT } from './partial-view-workspace.context.js';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { Subject, debounceTime } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('umb-partial-view-workspace-edit')
export class UmbPartialViewWorkspaceEditElement extends UmbLitElement {
	#name: string | undefined = '';
	@state()
	private get _name() {
		return this.#name;
	}

	private set _name(value) {
		this.#name = value?.replace('.cshtml', '');
		this.requestUpdate();
	}

	@state()
	private _content?: string | null = '';

	@state()
	private _path?: string | null = '';

	@state()
	private _ready?: boolean = false;

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#partialViewWorkspaceContext?: typeof UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT.TYPE;
	private _modalContext?: UmbModalManagerContext;

	private inputQuery$ = new Subject<string>();

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.consumeContext(UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#partialViewWorkspaceContext = workspaceContext;
			this.observe(this.#partialViewWorkspaceContext.name, (name) => {
				this._name = name;
			});

			this.observe(this.#partialViewWorkspaceContext.content, (content) => {
				this._content = content;
			});

			this.observe(this.#partialViewWorkspaceContext.path, (path) => {
				this._path = path;
			});

			this.observe(this.#partialViewWorkspaceContext.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});

			this.inputQuery$.pipe(debounceTime(250)).subscribe((nameInputValue: string) => {
				this.#partialViewWorkspaceContext?.setName(`${nameInputValue}.cshtml`);
			});
		});
	}

	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.inputQuery$.next(value);
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#partialViewWorkspaceContext?.setContent(value);
	}

	#insertSnippet(event: Event) {
		const target = event.target as UmbTemplatingInsertMenuElement;
		const value = target.value as string;
		this._codeEditor?.insert(value);
	}

	#openQueryBuilder() {
		const queryBuilderModal = this._modalContext?.open(UMB_TEMPLATE_QUERY_BUILDER_MODAL);

		queryBuilderModal?.onSubmit().then((queryBuilderModalResult) => {
			if (queryBuilderModalResult.value) this._codeEditor?.insert(getQuerySnippet(queryBuilderModalResult.value));
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
		return html`<umb-workspace-editor alias="Umb.Workspace.Template">
			<div id="workspace-header" slot="header">
				<uui-input
					placeholder="Enter name..."
					.value=${this._name}
					@input=${this.#onNameInput}
					label="template name"></uui-input>
				<small>Views/Partials/${this._path}</small>
			</div>
			<uui-box>
				<div slot="header" id="code-editor-menu-container">
					<umb-templating-insert-menu @insert=${this.#insertSnippet}></umb-templating-insert-menu>
					<uui-button look="secondary" id="query-builder-button" label="Query builder" @click=${this.#openQueryBuilder}>
						<uui-icon name="umb:wand"></uui-icon>Query builder
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

			#code-editor-menu-container uui-icon:not([name='umb:delete']) {
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

export default UmbPartialViewWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-workspace-edit': UmbPartialViewWorkspaceEditElement;
	}
}
