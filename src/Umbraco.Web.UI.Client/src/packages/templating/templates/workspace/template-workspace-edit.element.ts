import { UUITextStyles, UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_CONTEXT_TOKEN, UMB_TEMPLATE_PICKER_MODAL, UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbTemplatingInsertMenuElement } from '../../components/insert-menu/templating-insert-menu.element.js';
import { UMB_MODAL_TEMPLATING_INSERT_SECTION_MODAL } from '../../modals/insert-section-modal/insert-section-modal.element.js';
import { UmbTemplateWorkspaceContext } from './template-workspace.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
//import { UmbCodeEditorElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-template-workspace-edit')
export class UmbTemplateWorkspaceEditElement extends UmbLitElement {
	@state()
	private _name?: string | null = '';

	@state()
	private _content?: string | null = '';

	@state()
	private _masterTemplateName?: string | null = null;

	@query('umb-code-editor')
	private _codeEditor?: any;

	#templateWorkspaceContext?: UmbTemplateWorkspaceContext;
	#isNew = false;

	#masterTemplateId: string | null = null;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.consumeContext('UmbEntityWorkspaceContext', (workspaceContext: UmbTemplateWorkspaceContext) => {
			this.#templateWorkspaceContext = workspaceContext;
			this.observe(this.#templateWorkspaceContext.name, (name) => {
				this._name = name;
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
		});
	}

	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#templateWorkspaceContext?.setName(value);
	}

	#onCodeEditorInput(event: Event) {
		const target = event.target as any;
		const value = target.code as string;
		this.#templateWorkspaceContext?.setContent(value);
	}

	#insertSnippet(event: Event) {
		const target = event.target as UmbTemplatingInsertMenuElement;
		const value = target.value as string;
		this._codeEditor?.insert(value);
	}

	private _modalContext?: UmbModalContext;

	//TODO: fix this
	#openInsertSectionModal() {
		const sectionModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_SECTION_MODAL);
		sectionModal?.onSubmit().then((insertSectionModalResult) => {
			console.log(insertSectionModalResult);
		});
	}

	#resetMasterTemplate() {
		this.#setMasterTemplateId(null);
	}

	async #setMasterTemplateId(id: string | null) {
		if (this._content === null || this._content === undefined) return;
		const RegexString = /(@{[\s\S][^if]*?Layout\s*?=\s*?)("[^"]*?"|null)(;[\s\S]*?})/gi;

		if (id === null) {
			const string = this._content?.replace(RegexString, `$1null$3`);
			this.#templateWorkspaceContext?.setContent(string);
			return;
		}

		const masterTemplate = await this.#templateWorkspaceContext?.setMasterTemplate(id);

		const string = this._content?.replace(RegexString, `$1"${masterTemplate?.name}.cshtml"$3`);
		this.#templateWorkspaceContext?.setContent(string);
	}

	#openMasterTemplatePicker() {
		const modalHandler = this._modalContext?.open(UMB_TEMPLATE_PICKER_MODAL, {
			selection: [this.#masterTemplateId],
			pickableFilter: (item) => {
				return item.id !== this.#templateWorkspaceContext?.getEntityId();
			},
		});

		modalHandler?.onSubmit().then((data) => {
			if (!data.selection) return;
			this.#setMasterTemplateId(data.selection[0] ?? '');
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
					>Master template: ${this._masterTemplateName ?? 'No master'}</uui-button
				>
				<uui-button look="secondary" id="save-button" label="Remove master template" compact
					><uui-icon name="umb:delete" @click=${this.#resetMasterTemplate}></uui-icon
				></uui-button>
			</uui-button-group>
		`;
	}

	render() {
		// TODO: add correct UI elements
		return html`<umb-workspace-editor alias="Umb.Workspace.Template">
			<uui-input slot="header" .value=${this._name} @input=${this.#onNameInput}></uui-input>
			<uui-box>
				<div slot="header" id="code-editor-menu-container">
					${this.#renderMasterTemplatePicker()}
					<div>
						<umb-templating-insert-menu @insert=${this.#insertSnippet}></umb-templating-insert-menu>
						<uui-button look="secondary" id="query-builder-button" label="Query builder">
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

				<umb-code-editor
					language="razor"
					id="content"
					.code=${this._content ?? ''}
					@input=${this.#onCodeEditorInput}></umb-code-editor>
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

			umb-code-editor {
				margin-top: var(--uui-size-space-3);
				--editor-height: calc(100dvh - 300px);
			}

			uui-box {
				margin: var(--uui-size-layout-1);
				--uui-box-default-padding: 0;
			}

			uui-input {
				width: 100%;
				margin: 1em;
			}

			#code-editor-menu-container uui-icon {
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

export default UmbTemplateWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-workspace-edit': UmbTemplateWorkspaceEditElement;
	}
}
