import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../entity-detail-workspace.context-token.js';
import { customElement, html, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-entity-detail-workspace-editor')
export class UmbEntityDetailWorkspaceEditorElement extends UmbLitElement {
	#context?: typeof UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _isLoading = false;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context.loading.isOn, (isLoading) => {
				this._isLoading = isLoading;
			});
		});
	}

	protected override render() {
		return html`<umb-workspace-editor ?loading=${this._isLoading}>
			<slot name="header" slot="header"></slot>
		</umb-workspace-editor>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-detail-workspace-editor': UmbEntityDetailWorkspaceEditorElement;
	}
}
