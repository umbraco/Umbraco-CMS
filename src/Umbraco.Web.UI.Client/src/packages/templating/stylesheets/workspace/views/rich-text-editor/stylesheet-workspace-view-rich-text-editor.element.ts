import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import {
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalContext,
	UmbModalManagerContext,
	UmbModalToken,
} from '@umbraco-cms/backoffice/modal';
import { RichTextRuleModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR } from '../../manifests.js';
import { StylesheetRichTextEditorStyleModalResult } from './stylesheet-workspace-view-rich-text-editor-style-sidebar.js';

export const UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR_MODAL = new UmbModalToken<{ rule: RichTextRuleModel }>(
	UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR,
	{
		type: 'sidebar',
		size: 'medium',
	},
);
@customElement('umb-stylesheet-workspace-view-rich-text-editor')
export class UmbStylesheetWorkspaceViewRichTextEditorElement extends UmbLitElement {
	@state()
	private _content?: string | null = '';

	@state()
	private _path?: string | null = '';

	@state()
	private _ready?: boolean = false;

	@state()
	private _rules?: RichTextRuleModel[] = [
		{
			name: 'bjjh',
			selector: 'h1',
			styles: 'color: blue;',
		},
		{
			name: 'comeone',
			selector: 'h1',
			styles: 'color: blue;',
		},
		{
			name: 'lol',
			selector: 'h1',
			styles: 'color: blue;',
		},
	];

	#stylesheetWorkspaceContext?: UmbStylesheetWorkspaceContext;
	private _modalContext?: UmbModalManagerContext;

	#isNew = false;
	#modal?: UmbModalContext;

	constructor() {
		super();

		//tODO: should this be called something else here?
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#stylesheetWorkspaceContext = workspaceContext as UmbStylesheetWorkspaceContext;

			this.observe(this.#stylesheetWorkspaceContext.content, (content) => {
				this._content = content;
			});

			this.observe(this.#stylesheetWorkspaceContext.path, (path) => {
				this._path = path;
			});

			this.observe(this.#stylesheetWorkspaceContext.isNew, (isNew) => {
				this.#isNew = !!isNew;
			});

			this.observe(this.#stylesheetWorkspaceContext.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#openModal = (rule: RichTextRuleModel = {}) => {
		if (!this._modalContext) throw new Error('Modal context not found');
		const modal = this._modalContext.open(UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR_MODAL, {
			rule,
		});
		modal?.onSubmit().then((closedModal) => {
			console.log(closedModal);
		});
	};

	#removeRule = (rule: RichTextRuleModel) => {
		this._rules = this._rules?.filter((r) => r !== rule);
		throw new Error('Method not implemented.');
	};

	renderRule(rule: RichTextRuleModel) {
		return html`<div class="rule">
			<div class="rule-name"><uui-icon name="umb:navigation"></uui-icon>${rule.name}</div>
			<div class="rule-actions">
				<uui-button label="Edit" look="secondary" @click=${() => this.#openModal(rule)}>Edit</uui-button
				><uui-button label="Remove" look="secondary" color="danger" @click=${() => this.#removeRule(rule)}
					>Remove</uui-button
				>
			</div>
		</div>`;
	}
	render() {
		return html` <uui-box headline="Rich text editor styles">
			<div id="box-row">
				<p id="description">Define the styles that should be available in the rich text editor for this stylesheet.</p>
				<div id="rules">
					${this._rules?.map((rule) => this.renderRule(rule))}
					<uui-button label="Add rule" look="primary" @click=${this.#openModal}>Add</uui-button>
				</div>
			</div>
		</uui-box>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
			}

			#box-row {
				display: flex;
				gap: var(--uui-size-layout-1);
			}

			#description {
				margin-top: 0;
				flex: 0 0 250px;
			}

			#rules {
				flex: 1 1 auto;
				max-width: 600px;
			}

			.rule-name {
				display: flex;
				align-items: center;
				gap: var(--uui-size-2);
				padding-left: var(--uui-size-2);
				font-weight: bold;
			}

			.rule {
				display: flex;
				width: 100%;
				justify-content: space-between;
				padding: var(--uui-size-2);
				align-items: center;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-surface-alt);
				margin-bottom: var(--uui-size-space-4);
			}

			uui-box {
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbStylesheetWorkspaceViewRichTextEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace-view-rich-text-editor': UmbStylesheetWorkspaceViewRichTextEditorElement;
	}
}
