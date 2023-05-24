import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputElement } from '@umbraco-ui/uui';
import { UmbTemplatingInsertMenuElement } from '../../components/insert-menu/templating-insert-menu.element.js';
import { UMB_MODAL_TEMPLATING_INSERT_SECTION_MODAL } from '../../modals/insert-section-modal/insert-section-modal.element.js';
import { UmbTemplateWorkspaceContext } from './template-workspace.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalContext } from '@umbraco-cms/backoffice/modal';
//import { UmbCodeEditorElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-template-workspace-edit')
export class UmbTemplateWorkspaceEditElement extends UmbLitElement {
	@state()
	private _name?: string | null = '';

	@state()
	private _content?: string | null = '';

	@query('umb-code-editor')
	private _codeEditor?: any;

	#templateWorkspaceContext?: UmbTemplateWorkspaceContext;
	#isNew = false;

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

			this.observe(this.#templateWorkspaceContext.isNew, (isNew) => {
				this.#isNew = !!isNew;
			});
		});
	}

	// TODO: temp code for testing create and save
	#onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#templateWorkspaceContext?.setName(value);
	}

	//TODO - debounce that
	#onCodeEditorInput(event: Event) {
		const target = event.target as any;
		const value = target.code as string;
		this.#templateWorkspaceContext?.setContent(value);
	}

	#insertCode(event: Event) {
		const target = event.target as UmbTemplatingInsertMenuElement;
		const value = target.value as string;

		this._codeEditor?.insert(value);
	}

	private _modalContext?: UmbModalContext;

	#openInsertSectionModal() {
		const sectionModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_SECTION_MODAL);
		sectionModal?.onSubmit().then((insertSectionModalResult) => {
			console.log(insertSectionModalResult);
		});
	}

	render() {
		// TODO: add correct UI elements
		return html`<umb-body-layout alias="Umb.Workspace.Template">
			<uui-input slot="header" .value=${this._name} @input=${this.#onNameInput}></uui-input>
			<uui-box>
				<div slot="header" id="code-editor-menu-container">
					<uui-button-group>
						<uui-button look="secondary" id="master-template-button" label="Change Master template"
							>Master template: something</uui-button
						>
						<uui-button look="secondary" id="save-button" label="Remove master template" compact
							><uui-icon name="umb:delete"></uui-icon
						></uui-button>
					</uui-button-group>
					<umb-templating-insert-menu @insert=${this.#insertCode}></umb-templating-insert-menu>
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

				<umb-code-editor
					language="razor"
					id="content"
					.code=${this._content ?? ''}
					@input=${this.#onCodeEditorInput}></umb-code-editor>
			</uui-box>
		</umb-body-layout>`;
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
				--editor-height: calc(100vh - 300px);
			}

			uui-box {
				margin: 1em;
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
				justify-content: flex-end;
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
