import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';
import '../../../../shared/components/workspace/actions/save/workspace-action-node-save.element.ts';

@customElement('umb-log-search-workspace')
export class UmbLogSearchWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}

			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}
		`,
	];

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.LogSearch">
				<div id="header" slot="header">
					<a href="/section/settings/language-root">
						<uui-button compact>
							<uui-icon name="umb:arrow-left"></uui-icon>
						</uui-button>
					</a>
					<uui-input></uui-input>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbLogSearchWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-search-workspace': UmbLogSearchWorkspaceElement;
	}
}
