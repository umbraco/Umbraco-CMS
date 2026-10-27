import { UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT } from './partial-view-workspace.context-token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-partial-view-workspace-editor')
export class UmbPartialViewWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _isNew?: boolean;

	@state()
	private _isReadOnly = false;

	#workspaceContext?: typeof UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.observe(context?.readOnly, (isReadOnly) => (this._isReadOnly = isReadOnly ?? false), '_observeIsReadOnly');
		});

		this.consumeContext(UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;

			this.observe(this.#workspaceContext?.isNew, (isNew) => {
				this._isNew = isNew;
			});
		});
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-workspace-header-name-editable
					slot="header"
					?readonly=${this._isNew === false || this._isReadOnly}></umb-workspace-header-name-editable>
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbPartialViewWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-workspace-editor': UmbPartialViewWorkspaceEditorElement;
	}
}
