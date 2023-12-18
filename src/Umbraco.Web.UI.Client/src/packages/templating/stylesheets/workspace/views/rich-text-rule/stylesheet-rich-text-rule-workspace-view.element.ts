import { UmbStylesheetWorkspaceContext } from '../../stylesheet-workspace.context.js';
import { UmbStylesheetRichTextRuleRepository } from '../../../repository/index.js';
import { UmbSortableStylesheetRule } from '../../../types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-stylesheet-rich-text-rule-workspace-view')
export class UmbStylesheetRichTextRuleWorkspaceViewElement extends UmbLitElement {
	@state()
	_rules: UmbSortableStylesheetRule[] = [];

	#context?: UmbStylesheetWorkspaceContext;

	#stylesheetRichTextRuleRepository = new UmbStylesheetRichTextRuleRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#context = workspaceContext as UmbStylesheetWorkspaceContext;
			const unique = this.#context?.getEntityId();
			this.#setRules(unique);
		});
	}

	async #setRules(unique: string) {
		const { data } = await this.#stylesheetRichTextRuleRepository.requestStylesheetRules(unique);

		if (data) {
			this._rules = data.rules ?? [];
		}
	}

	#onRuleChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbStylesheetRuleInputElement;
		const rules = target.rules;
		console.log(rules);
		console.log(event);
		debugger;
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
