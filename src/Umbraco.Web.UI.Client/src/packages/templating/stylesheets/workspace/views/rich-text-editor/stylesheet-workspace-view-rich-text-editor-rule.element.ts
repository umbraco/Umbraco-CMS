import { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR_MODAL } from './stylesheet-workspace-view-rich-text-editor.element.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { RichTextRuleModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';

@customElement('umb-stylesheet-rich-text-editor-rule')
export default class UmbStylesheetRichTextEditorRuleElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	private rule: RichTextRuleModel | null = null;

	#context?: UmbStylesheetWorkspaceContext;
	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#context = workspaceContext as UmbStylesheetWorkspaceContext;
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	openModal = () => {
		if (!this._modalContext) throw new Error('Modal context not found');
		const modal = this._modalContext.open(UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR_MODAL, {
			rule: this.rule,
		});
		modal?.onSubmit().then((result) => {
			if (result.rule && this.rule?.name) {
				this.#context?.updateRule(this.rule?.name, result.rule);
			}
		});
	};

	removeRule = () => {
		//TODO: SPORTER BREAKS THAT - rules are removed from the data but not from the DOM
		if (!this.#context) throw new Error('Context not found');
		this.#context.setRules(this.#context.getRules().filter((r) => r.name !== this.rule?.name));
	};

	render() {
		return html`
			<div class="rule-name"><uui-icon name="umb:navigation"></uui-icon>${this.rule?.name}</div>
			<div class="rule-actions">
				<uui-button label="Edit" look="secondary" @click=${this.openModal}>Edit</uui-button
				><uui-button label="Remove" look="secondary" color="danger" @click=${this.removeRule}>Remove</uui-button>
			</div>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				width: 100%;
				justify-content: space-between;
				padding: var(--uui-size-2);
				align-items: center;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-surface-alt);
				margin-bottom: var(--uui-size-space-4);
			}

			.rule-name {
				display: flex;
				align-items: center;
				gap: var(--uui-size-2);
				padding-left: var(--uui-size-2);
				font-weight: bold;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-rich-text-editor-rule': UmbStylesheetRichTextEditorRuleElement;
	}
}
