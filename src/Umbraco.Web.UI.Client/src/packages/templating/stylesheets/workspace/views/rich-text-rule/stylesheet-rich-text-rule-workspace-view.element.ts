import { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { UmbSortableStylesheetRule } from '../../../types.js';
import { UmbStylesheetRuleInputElement } from '../../../components/index.js';
import { UmbStylesheetRuleManager } from '../../../utils/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-stylesheet-rich-text-rule-workspace-view')
export class UmbStylesheetRichTextRuleWorkspaceViewElement extends UmbLitElement {
	@state()
	_rules: UmbSortableStylesheetRule[] = [];

	#context?: UmbStylesheetWorkspaceContext;
	#stylesheetRuleManager = new UmbStylesheetRuleManager();
	#stylesheetContent = '';

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#context = workspaceContext as UmbStylesheetWorkspaceContext;
			this.#observeContent();
		});
	}

	#observeContent() {
		if (!this.#context?.content) return;
		this.observe(
			this.#context.content,
			(content) => {
				this.#stylesheetContent = content;
				this.#extractRules(content);
			},
			'umbStylesheetContentObserver',
		);
	}

	#extractRules(content: string | undefined) {
		if (content) {
			const rules = this.#stylesheetRuleManager.extractRules(content);
			this._rules = [...rules];
		} else {
			this._rules = [];
		}
	}

	#onRuleChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbStylesheetRuleInputElement;
		const rules = target.rules;
		const newContent = this.#stylesheetRuleManager.insertRules(this.#stylesheetContent, rules);
		this.#context?.setContent(newContent);
	}

	render() {
		return html`<uui-box headline="Rich text editor styles">
			<umb-property-layout
				description="Define the styles that should be available in the rich text editor for this stylesheet.">
				<div slot="editor">
					<umb-stylesheet-rule-input .rules=${this._rules} @change=${this.#onRuleChange}></umb-stylesheet-rule-input>
				</div>
			</umb-property-layout>
		</uui-box>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
			}

			uui-box {
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbStylesheetRichTextRuleWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-rich-text-rule-workspace-view': UmbStylesheetRichTextRuleWorkspaceViewElement;
	}
}
