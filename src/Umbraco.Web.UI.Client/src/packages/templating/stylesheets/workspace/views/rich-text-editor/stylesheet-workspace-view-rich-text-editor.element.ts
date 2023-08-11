import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { RichTextRuleModel } from '@umbraco-cms/backoffice/backend-api';

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
			"name": "bjjh",
			"selector": "h1",
			"styles": "color: blue;"
		},
		{
			"name": "comeone",
			"selector": "h1",
			"styles": "color: blue;"
		},
		{
			"name": "lol",
			"selector": "h1",
			"styles": "color: blue;"
		}
	];

	#stylesheetWorkspaceContext?: UmbStylesheetWorkspaceContext;
	private _modalContext?: UmbModalManagerContext;

	#isNew = false;

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
	}

	renderRule(rule: RichTextRuleModel) {
		return html`<div class="rule">
			<div class="rule-name"><uui-icon name="umb:move"></uui-icon>${rule.name}</div>
			<div class="rule-actions"><uui-button label="Edit" look="secondary">Edit</uui-button><uui-button label="Remove" look="secondary" color="danger">Remove</uui-button></div>
		</div>`;
	}
	render() {
		return html` <uui-box headline="Rich text editor styles">

		<div id="box-row">
		
		<div id="description"><p>Define the styles that should be available in the rich text editor for this stylesheet</p></div>
		<div id="rules">
			${this._rules?.map((rule) => this.renderRule(rule))}
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
				flex: 0 0 250px
			}

			#rules {
				flex: 1 1 auto;
				max-width: 600px;
			}

			.rule {
				display: flex;
				width: 100%;
				justify-content: space-between;
				padding: var(--uui-size-2);
				align-items: center;
				border-radius: var(--uui-size-border-radius);
				background-color: var(--uui-color-surface-alt);
				margin-bottom: var(--uui-size-space-4);
			}

			uui-box {
				margin: var(--uui-size-layout-1);
				/* remove header border bottom as code editor looks better in this box */
				--uui-color-divider-standalone: transparent;
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
