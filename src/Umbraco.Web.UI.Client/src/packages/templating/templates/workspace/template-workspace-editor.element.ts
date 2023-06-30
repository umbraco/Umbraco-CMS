import type { UmbTemplatingInsertMenuElement } from '../../components/insert-menu/templating-insert-menu.element.js';
import { UMB_MODAL_TEMPLATING_INSERT_SECTION_MODAL } from '../../modals/insert-section-modal/insert-section-modal.element.js';
import type { UmbTemplateWorkspaceContext } from './template-workspace.context.js';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { camelCase } from '@umbraco-cms/backoffice/external/lodash';
import { UUITextStyles, UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, query, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import {
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_TEMPLATE_PICKER_MODAL,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { Subject, debounceTime } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_MODAL_TEMPLATING_QUERY_BUILDER_SIDEBAR_ALIAS } from '../modals/manifests.js';
import { UMB_TEMPLATE_QUERY_BUILDER_MODAL } from '../modals/modal-tokens.js';
import { getQuerySnippet } from '../../utils.js';

@customElement('umb-template-workspace-editor')
export class UmbTemplateWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string | null = '';

	@state()
	private _content?: string | null = '';

	@state()
	private _alias?: string | null = '';

	@state()
	private _ready?: boolean = false;

	@state()
	private _masterTemplateName?: string | null = null;

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#templateWorkspaceContext?: UmbTemplateWorkspaceContext;
	#isNew = false;

	#masterTemplateId: string | null = null;

	private inputQuery$ = new Subject<string>();

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.consumeContext('UmbEntityWorkspaceContext', (workspaceContext: UmbTemplateWorkspaceContext) => {
			this.#templateWorkspaceContext = workspaceContext;
			this.observe(this.#templateWorkspaceContext.name, (name) => {
				this._name = name;
			});

			this.observe(this.#templateWorkspaceContext.alias, (alias) => {
				this._alias = alias;
			});

			this.observe(this.#templateWorkspaceContext.content, (content) => {
				this._content = content;
			});

			this.observe(this.#templateWorkspaceContext.masterTemplate, (masterTemplate) => {
				this.#masterTemplateId = masterTemplate?.id ?? null;
				this._masterTemplateName = masterTemplate?.name ?? null;
			});

			this.observe(this.#templateWorkspaceContext.isNew, (isNew) => {
				this.#isNew = !!isNew;
			});

			this.observe(this.#templateWorkspaceContext.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});

			this.inputQuery$.pipe(debounceTime(250)).subscribe((nameInputValue) => {
				this.#templateWorkspaceContext?.setName(nameInputValue);
				if (this.#isNew && !this._alias) this.#templateWorkspaceContext?.setAlias(camelCase(nameInputValue));
			});
		});
	}

	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.inputQuery$.next(value);
	}

	#onAliasInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#templateWorkspaceContext?.setAlias(value);
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as UmbCodeEditorElement;
		const value = target.code as string;
		this.#templateWorkspaceContext?.setContent(value);
	}

	#insertSnippet(event: Event) {
		const target = event.target as UmbTemplatingInsertMenuElement;
		const value = target.value as string;
		this._codeEditor?.insert(value);
	}

	private _modalContext?: UmbModalManagerContext;

	#openInsertSectionModal() {
		const sectionModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_SECTION_MODAL);
		sectionModal?.onSubmit().then((insertSectionModalResult) => {
			if (insertSectionModalResult.value) this._codeEditor?.insert(insertSectionModalResult.value);
		});
	}

	#resetMasterTemplate() {
		this.#templateWorkspaceContext?.setMasterTemplate(null);
	}

	#openMasterTemplatePicker() {
		const modalContext = this._modalContext?.open(UMB_TEMPLATE_PICKER_MODAL, {
			selection: [this.#masterTemplateId],
			pickableFilter: (item) => {
				return item.id !== null && item.id !== this.#templateWorkspaceContext?.getEntityId();
			},
		});

		modalContext?.onSubmit().then((data) => {
			if (!data.selection) return;
			this.#templateWorkspaceContext?.setMasterTemplate(data.selection[0] ?? '');
		});
	}

	#openQueryBuilder() {
		const queryBuilderModal = this._modalContext?.open(UMB_TEMPLATE_QUERY_BUILDER_MODAL);

		queryBuilderModal?.onSubmit().then((queryBuilderModalResult) => {
			if (queryBuilderModalResult.value) this._codeEditor?.insert(getQuerySnippet(queryBuilderModalResult.value));
		});
	}

	#renderMasterTemplatePicker() {
		return html`
			<uui-button-group>
				<uui-button
					@click=${this.#openMasterTemplatePicker}
					look="secondary"
					id="master-template-button"
					label="Change Master template"
					>${this._masterTemplateName
						? `Master template: ${this._masterTemplateName}`
						: 'Set master template'}</uui-button
				>
				${this._masterTemplateName
					? html` <uui-button look="secondary" id="save-button" label="Remove master template" compact
							><uui-icon name="umb:delete" @click=${this.#resetMasterTemplate}></uui-icon
					  ></uui-button>`
					: nothing}
			</uui-button-group>
		`;
	}

	#renderCodeEditor() {
		return html`<umb-code-editor
			language="razor"
			id="content"
			.code=${this._content ?? ''}
			@input=${this.#onCodeEditorInput}></umb-code-editor>`;
	}

	render() {
		// TODO: add correct UI elements
		return html`<umb-workspace-editor alias="Umb.Workspace.Template">
			<uui-input
				placeholder="Enter name..."
				slot="header"
				.value=${this._name}
				@input=${this.#onNameInput}
				label="template name"
				><umb-template-alias-input
					slot="append"
					.value=${this._alias ?? ''}
					@change=${this.#onAliasInput}></umb-template-alias-input
			></uui-input>
			<uui-box>
				<div slot="header" id="code-editor-menu-container">
					${this.#renderMasterTemplatePicker()}
					<div>
						<umb-templating-insert-menu @insert=${this.#insertSnippet}></umb-templating-insert-menu>
						<uui-button
							look="secondary"
							id="query-builder-button"
							label="Query builder"
							@click=${this.#openQueryBuilder}>
							<uui-icon name="umb:wand"></uui-icon>Query builder
						</uui-button>
						<uui-button
							look="secondary"
							id="sections-button"
							label="Query builder"
							@click=${this.#openInsertSectionModal}>
							<uui-icon name="umb:indent"></uui-icon>Sections
						</uui-button>
					</div>
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
		UUITextStyles,
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

			uui-input {
				width: 100%;
				margin: 1em;
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
				justify-content: space-between;
				gap: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbTemplateWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-workspace-editor': UmbTemplateWorkspaceEditorElement;
	}
}
