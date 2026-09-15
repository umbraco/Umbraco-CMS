import { UMB_STYLESHEET_WORKSPACE_CONTEXT } from './stylesheet-workspace.context-token.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-stylesheet-workspace-editor')
export class UmbStylesheetWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _isNew?: boolean;

	#context?: typeof UMB_STYLESHEET_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_STYLESHEET_WORKSPACE_CONTEXT, (context) => {
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

export default UmbStylesheetWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace-editor': UmbStylesheetWorkspaceEditorElement;
	}
}
