import { getQuerySnippet } from '../../../../utils/index.js';
import { UMB_TEMPLATE_QUERY_BUILDER_MODAL } from '../../../modals/query-builder/index.js';
import { UMB_TEMPLATING_SECTION_PICKER_MODAL } from '../../../../modals/templating-section-picker/templating-section-picker-modal.token.js';
import type { UmbTemplatingInsertMenuElement } from '../../../../local-components/insert-menu/insert-menu.element.js';
import { UMB_TEMPLATE_PICKER_MODAL } from '../../../modals/index.js';
import { UMB_TEMPLATE_WORKSPACE_CONTEXT } from '../../template-workspace.context-token.js';
import { css, customElement, html, nothing, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

import '@umbraco-cms/backoffice/code-editor';
import '../../../../local-components/insert-menu/index.js';

@customElement('umb-template-code-editor-workspace-view')
export class UmbTemplateCodeEditorWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#modalContext?: UmbModalManagerContext;

	@state()
	private _content?: string | null = '';

	@state()
	private _masterTemplateName?: string | null = null;

	/**
	 * Whether editing is restricted. True when in production mode OR when runtime mode is still unknown.
	 * This ensures a safe default (restricted) until we confirm the runtime mode.
	 */
	@state()
	private _isRestricted = true;

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#templateWorkspaceContext?: typeof UMB_TEMPLATE_WORKSPACE_CONTEXT.TYPE;

	#masterTemplateUnique: string | null = null;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});

		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.observe(context?.isProductionMode, (isProductionMode) => {
				// Restricted until we confirm it's NOT production mode (safe default).
				this._isRestricted = isProductionMode !== false;
			});
		});

		this.consumeContext(UMB_TEMPLATE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#templateWorkspaceContext = workspaceContext;

			this.observe(this.#templateWorkspaceContext?.content, (content) => {
				this._content = content;
			});

			this.observe(this.#templateWorkspaceContext?.masterTemplate, (masterTemplate) => {
				this.#masterTemplateUnique = masterTemplate?.unique ?? null;
				this._masterTemplateName = masterTemplate?.name ?? null;
			});
		});
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
		this.#templateWorkspaceContext?.setMasterTemplate(null, true);
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
				this.#templateWorkspaceContext?.setMasterTemplate(value.selection[0] ?? null, true);
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
					?disabled=${this._isRestricted}
					label="${this.localize.term('template_mastertemplate')}: ${this._masterTemplateName
						? this._masterTemplateName
						: this.localize.term('template_noMaster')}"></uui-button>
				${this._masterTemplateName
					? html`<uui-button
							look="secondary"
							label=${this.localize.term('actions_remove')}
							?disabled=${this._isRestricted}
							@click=${this.#resetMasterTemplate}
							compact>
							<uui-icon name="icon-delete"></uui-icon>
						</uui-button>`
					: nothing}
			</uui-button-group>
		`;
	}

	override render() {
		return html`
			<uui-box>
				<div slot="header" id="code-editor-menu-container">${this.#renderMasterTemplatePicker()}</div>
				<div slot="header-actions">
					<umb-templating-insert-menu @insert=${this.#insertSnippet} ?disabled=${this._isRestricted}>
					</umb-templating-insert-menu>
					<uui-button
						look="secondary"
						id="query-builder-button"
						label=${this.localize.term('template_queryBuilder')}
						?disabled=${this._isRestricted}
						@click=${this.#openQueryBuilder}>
						<uui-icon name="icon-wand"></uui-icon> ${this.localize.term('template_queryBuilder')}
					</uui-button>
					<uui-button
						look="secondary"
						id="sections-button"
						label=${this.localize.term('template_insertSections')}
						?disabled=${this._isRestricted}
						@click=${this.#openInsertSectionModal}>
						<uui-icon name="icon-indent"></uui-icon> ${this.localize.term('template_insertSections')}
					</uui-button>
				</div>

				${this.#renderCodeEditor()}
			</uui-box>
		`;
	}

	#renderCodeEditor() {
		return html`
			<umb-code-editor
				id="content"
				language="razor"
				.code=${this._content ?? ''}
				?readonly=${this._isRestricted}
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
				justify-content: space-between;
				gap: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbTemplateCodeEditorWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-code-editor-workspace-view': UmbTemplateCodeEditorWorkspaceViewElement;
	}
}
