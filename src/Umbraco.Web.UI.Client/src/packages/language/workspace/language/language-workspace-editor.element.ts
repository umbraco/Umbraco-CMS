import { UMB_LANGUAGE_ROOT_WORKSPACE_PATH } from '../language-root/paths.js';
import { UMB_LANGUAGE_WORKSPACE_CONTEXT } from './language-workspace.context-token.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
@customElement('umb-language-workspace-editor')
export class UmbLanguageWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_LANGUAGE_WORKSPACE_CONTEXT.TYPE;

	@state()
	_isNew?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_LANGUAGE_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.observe(this.#workspaceContext?.isNew, (isNew) => (this._isNew = isNew));
		});
	}

	override render() {
		return html`<umb-entity-detail-workspace-editor .backPath=${UMB_LANGUAGE_ROOT_WORKSPACE_PATH}>
			${this._isNew
				? html`<h3 slot="header">Add language</h3>`
				: html`<umb-workspace-header-name-editable slot="header"></umb-workspace-header-name-editable> `}
		</umb-entity-detail-workspace-editor>`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbLanguageWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace-editor': UmbLanguageWorkspaceEditorElement;
	}
}
