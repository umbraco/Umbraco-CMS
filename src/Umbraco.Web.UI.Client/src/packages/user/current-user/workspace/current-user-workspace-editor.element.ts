import { UMB_CURRENT_USER_WORKSPACE_CONTEXT } from './current-user-workspace.context-token.js';
import { customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './current-user-workspace-avatar.element.js';
import './current-user-workspace-settings.element.js';

@customElement('umb-current-user-workspace-editor')
export class UmbCurrentUserWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _headline?: string;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_WORKSPACE_CONTEXT, (context) => {
			if (!context) return;
			this.observe(context.name, (name) => (this._headline = name ?? ''), 'umbCurrentUserWorkspaceEditorNameObserver');
		});
	}

	override render() {
		return html`
			<umb-workspace-editor .headline=${this._headline ?? ''}>
				<umb-body-layout>
					<umb-stack look="compact">
						<umb-current-user-workspace-avatar></umb-current-user-workspace-avatar>
						<umb-current-user-workspace-settings></umb-current-user-workspace-settings>
					</umb-stack>
				</umb-body-layout>
			</umb-workspace-editor>
		`;
	}
}

export { UmbCurrentUserWorkspaceEditorElement as element };

export default UmbCurrentUserWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-workspace-editor': UmbCurrentUserWorkspaceEditorElement;
	}
}
