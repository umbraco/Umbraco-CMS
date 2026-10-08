import { UMB_SCRIPT_WORKSPACE_CONTEXT } from './script-workspace.context-token.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-script-workspace-editor')
export class UmbScriptWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _isNew?: boolean;

	#context?: typeof UMB_SCRIPT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_SCRIPT_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context?.isNew, (isNew) => (this._isNew = isNew));
		});
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-workspace-header-name-editable
					slot="header"
					?readonly=${this._isNew === false}></umb-workspace-header-name-editable>
			</umb-entity-detail-workspace-editor>
		`;
	}
}

export default UmbScriptWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-script-workspace-editor': UmbScriptWorkspaceEditorElement;
	}
}
