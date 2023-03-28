import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-language-workspace-create')
export class UmbLanguageWorkspaceCreateElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}
			strong {
				display: flex;
				align-items: center;
			}
		`,
	];

	render() {
		return html`<umb-workspace-layout alias="Umb.Workspace.Language">
			<div id="header" slot="header">
				<uui-button label="Nagivate back" href="/section/settings/workspace/language-root" compact>
					<uui-icon name="umb:arrow-left"></uui-icon>
				</uui-button>
				<strong>Add language</strong>
			</div>
		</umb-workspace-layout>`;
	}
}

export default UmbLanguageWorkspaceCreateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace-create': UmbLanguageWorkspaceCreateElement;
	}
}
