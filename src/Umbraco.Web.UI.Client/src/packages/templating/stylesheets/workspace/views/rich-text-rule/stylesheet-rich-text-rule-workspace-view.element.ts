import type { UmbStylesheetRule } from '../../../types.js';
import type { UmbStylesheetRuleInputElement } from '../../../global-components/index.js';
import { UmbStylesheetRuleManager } from '../../../utils/index.js';
import { UMB_STYLESHEET_WORKSPACE_CONTEXT } from '../../stylesheet-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-stylesheet-rich-text-rule-workspace-view')
export class UmbStylesheetRichTextRuleWorkspaceViewElement extends UmbLitElement {
	@state()
	_rules: UmbStylesheetRule[] = [];

	#context?: typeof UMB_STYLESHEET_WORKSPACE_CONTEXT.TYPE;
	#stylesheetRuleManager = new UmbStylesheetRuleManager();
	#stylesheetContent = '';

	constructor() {
		super();

		this.consumeContext(UMB_STYLESHEET_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.#observeContent();
		});
	}

	#observeContent() {
		if (!this.#context?.content) return;
		this.observe(
			this.#context.content,
			(content) => {
				this.#stylesheetContent = content || '';
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

	override render() {
		return html`<uui-box headline="Rich text editor styles">
			<umb-property-layout
				description="Define the styles that should be available in the rich text editor for this stylesheet.">
				<div slot="editor">
					<umb-stylesheet-rule-input .rules=${this._rules} @change=${this.#onRuleChange}></umb-stylesheet-rule-input>
				</div>
			</umb-property-layout>
		</uui-box>`;
	}

	static override styles = [
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
