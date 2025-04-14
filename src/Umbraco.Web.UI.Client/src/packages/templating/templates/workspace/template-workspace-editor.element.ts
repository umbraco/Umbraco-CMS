import { getQuerySnippet } from '../../utils/index.js';
import { UMB_TEMPLATE_QUERY_BUILDER_MODAL } from '../modals/query-builder/index.js';
import { UMB_TEMPLATING_SECTION_PICKER_MODAL } from '../../modals/templating-section-picker/templating-section-picker-modal.token.js';
import type { UmbTemplatingInsertMenuElement } from '../../local-components/insert-menu/insert-menu.element.js';
import { UMB_TEMPLATE_WORKSPACE_CONTEXT } from './template-workspace.context-token.js';
import { css, customElement, html, nothing, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import type { UmbInputWithAliasElement } from '@umbraco-cms/backoffice/components';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';

import '@umbraco-cms/backoffice/code-editor';
import '../../local-components/insert-menu/index.js';
import { UMB_TEMPLATE_PICKER_MODAL } from '../modals/index.js';

@customElement('umb-template-workspace-editor')
export class UmbTemplateWorkspaceEditorElement extends UmbLitElement {
	#modalContext?: UmbModalManagerContext;

	@state()
	private _name?: string = '';

	@state()
	private _content?: string | null = '';

	@state()
	private _alias?: string = '';

	@state()
	private _masterTemplateName?: string | null = null;

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#templateWorkspaceContext?: typeof UMB_TEMPLATE_WORKSPACE_CONTEXT.TYPE;
	#isNew = false;

	#masterTemplateUnique: string | null = null;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});

		this.consumeContext(UMB_TEMPLATE_WORKSPACE_CONTEXT, (workspaceContext) => {
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
				this.#masterTemplateUnique = masterTemplate?.unique ?? null;
				this._masterTemplateName = masterTemplate?.name ?? null;
			});

			this.observe(this.#templateWorkspaceContext.isNew, (isNew) => {
				this.#isNew = !!isNew;
			});
		});
	}

	#onNameAndAliasChange(event: InputEvent & { target: UmbInputWithAliasElement }) {
		this.#templateWorkspaceContext?.setName(event.target.value ?? '');
		this.#templateWorkspaceContext?.setAlias(event.target.alias ?? '');
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

	#openInsertSectionModal() {
		const sectionModal = this.#modalContext?.open(this, UMB_TEMPLATING_SECTION_PICKER_MODAL);

		sectionModal
			?.onSubmit()
			.then((insertSectionModalValue) => {
				if (insertSectionModalValue?.value) {
					this._codeEditor?.insert(insertSectionModalValue.value);
				}
			})
			.catch(() => undefined);
	}

	#resetMasterTemplate() {
		this.#templateWorkspaceContext?.setMasterTemplate(null);
	}

	#openMasterTemplatePicker() {
		const modalContext = this.#modalContext?.open(this, UMB_TEMPLATE_PICKER_MODAL, {
			data: {
				pickableFilter: (item) => {
					return item.unique !== null && item.unique !== this.#templateWorkspaceContext?.getUnique();
				},
			},
			value: {
				selection: [this.#masterTemplateUnique],
			},
		});

		modalContext
			?.onSubmit()
			.then((value) => {
				if (!value?.selection) return;
				this.#templateWorkspaceContext?.setMasterTemplate(value.selection[0] ?? null);
			})
			.catch(() => undefined);
	}

	#openQueryBuilder() {
		const queryBuilderModal = this.#modalContext?.open(this, UMB_TEMPLATE_QUERY_BUILDER_MODAL);

		queryBuilderModal
			?.onSubmit()
			.then((queryBuilderModalValue) => {
				if (queryBuilderModalValue?.value) {
					this._codeEditor?.insert(getQuerySnippet(queryBuilderModalValue.value));
				}
			})
			.catch(() => undefined);
	}

	#renderMasterTemplatePicker() {
		return html`
			<uui-button-group>
				<uui-button
					@click=${this.#openMasterTemplatePicker}
					look="secondary"
					id="master-template-button"
					label="${this.localize.term('template_mastertemplate')}: ${this._masterTemplateName
						? this._masterTemplateName
						: this.localize.term('template_noMaster')}"></uui-button>
				${this._masterTemplateName
					? html`<uui-button look="secondary" label=${this.localize.term('actions_remove')} compact>
							<uui-icon name="icon-delete" @click=${this.#resetMasterTemplate}></uui-icon>
						</uui-button>`
					: nothing}
			</uui-button-group>
		`;
	}

	override render() {
		// TODO: add correct UI elements
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-input-with-alias
					slot="header"
					id="name"
					label=${this.localize.term('placeholders_entername')}
					placeholder=${this.localize.term('placeholders_entername')}
					.value=${this._name}
					.alias=${this._alias}
					?auto-generate-alias=${this.#isNew}
					@change=${this.#onNameAndAliasChange}
					required
					${umbBindToValidation(this)}
					${umbFocus()}>
				</umb-input-with-alias>

				<uui-box>
					<div slot="header" id="code-editor-menu-container">${this.#renderMasterTemplatePicker()}</div>
					<div slot="header-actions">
						<umb-templating-insert-menu @insert=${this.#insertSnippet}></umb-templating-insert-menu>
						<uui-button
							look="secondary"
							id="query-builder-button"
							label=${this.localize.term('template_queryBuilder')}
							@click=${this.#openQueryBuilder}>
							<uui-icon name="icon-wand"></uui-icon> ${this.localize.term('template_queryBuilder')}
						</uui-button>
						<uui-button
							look="secondary"
							id="sections-button"
							label=${this.localize.term('template_insertSections')}
							@click=${this.#openInsertSectionModal}>
							<uui-icon name="icon-indent"></uui-icon> ${this.localize.term('template_insertSections')}
						</uui-button>
					</div>

					${this.#renderCodeEditor()}
				</uui-box>
			</umb-entity-detail-workspace-editor>
		`;
	}

	#renderCodeEditor() {
		return html`
			<umb-code-editor
				id="content"
				language="razor"
				.code=${this._content ?? ''}
				@input=${this.#onCodeEditorInput}></umb-code-editor>
		`;
	}

	static override styles = [
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

			umb-input-with-alias {
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
