import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR } from '../../manifests.js';
import {
	StylesheetRichTextEditorStyleModalData,
	StylesheetRichTextEditorStyleModalResult,
} from './stylesheet-workspace-view-rich-text-editor-style-sidebar.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import {
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalContext,
	UmbModalManagerContext,
	UmbModalToken,
} from '@umbraco-cms/backoffice/modal';
import { RichTextRuleModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';

export const UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR_MODAL = new UmbModalToken<
	StylesheetRichTextEditorStyleModalData,
	StylesheetRichTextEditorStyleModalResult
>(UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR, {
	type: 'sidebar',
	size: 'medium',
});

const SORTER_CONFIG: UmbSorterConfig<RichTextRuleModel> = {
	compareElementToModel: (element: HTMLElement, model: RichTextRuleModel) => {
		return element.getAttribute('data-umb-rule-name') === model.name;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: RichTextRuleModel) => {
		return container.querySelector('data-umb-rule-name[' + modelEntry.name + ']');
	},
	identifier: 'stylesheet-rules-sorter',
	itemSelector: '[data-umb-rule-name]',
	disabledItemSelector: '[inherited]',
	containerSelector: '#rules-container',
};

@customElement('umb-stylesheet-workspace-view-rich-text-editor')
export class UmbStylesheetWorkspaceViewRichTextEditorElement extends UmbLitElement {
	@state()
	private _content: string = '';

	@state()
	private _path: string = '';

	@state()
	private _ready: boolean = false;

	@state()
	_rules: RichTextRuleModel[] = [];

	#context?: UmbStylesheetWorkspaceContext;
	private _modalContext?: UmbModalManagerContext;

	#isNew = false;
	#modal?: UmbModalContext;

	#currentlyEditing: RichTextRuleModel | null = null;

	#sorter = new UmbSorterController(this, {
		...SORTER_CONFIG,
		performItemInsert: ({ item, newIndex }) => {
			//return true;

			return this.#context?.findNewSortOrder(item, newIndex) ?? false;
		},
		performItemRemove: (args) => {
			console.log(args, 'remove');
			return true;
		},
	});

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#context = workspaceContext as UmbStylesheetWorkspaceContext;
			this.#context.sendContentGetRules();

			this.observe(this.#context.content, (content) => {
				this._content = content ?? '';
			});

			this.observe(this.#context.path, (path) => {
				this._path = path ?? '';
			});

			this.observe(this.#context.isNew, (isNew) => {
				this.#isNew = !!isNew;
			});

			this.observe(this.#context.isCodeEditorReady, (isReady) => {
				this._ready = isReady;
			});

			this.observe(this.#context.rules, (rules) => {
				this._rules = rules;
				this.#sorter.setModel(this._rules);
			});
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#openModal = (rule: RichTextRuleModel | null = null) => {
		if (!this._modalContext) throw new Error('Modal context not found');
		this.#currentlyEditing = rule;
		const modal = this._modalContext.open(UMB_MODAL_TEMPLATING_STYLESHEET_RTF_STYLE_SIDEBAR_MODAL, {
			rule,
		});
		modal?.onSubmit().then((result) => {
			if (this.#currentlyEditing && result.rule) {
				this.#replaceRule(result.rule);
				this.#currentlyEditing = null;
				return;
			}
			if (result.rule) {
				this.#context?.setRules([...this._rules, result.rule]);
				this.#currentlyEditing = null;
			}
		});
	};

	#removeRule = (rule: RichTextRuleModel) => {
		this.#context?.setRules(this._rules?.filter((r) => r !== rule));
	};

	#replaceRule(rule: RichTextRuleModel) {
		this.#context?.setRules(this._rules?.map((r) => (r === this.#currentlyEditing ? rule : r)));
	}

	renderRule(rule: RichTextRuleModel) {
		return html`<div class="rule" data-umb-rule-name="${ifDefined(rule.name)}">
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
					<div id="rules-container">${repeat(this._rules, (rule) => rule.name, this.renderRule)}</div>
					<uui-button label="Add rule" look="primary" @click=${() => this.#openModal(null)}>Add</uui-button>
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
