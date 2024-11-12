import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../entity-detail-workspace.context-token.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-entity-detail-workspace-editor')
export class UmbEntityDetailWorkspaceEditorElement extends UmbLitElement {
	@property({ attribute: 'back-path' })
	public backPath?: string;

	@state()
	private _entityType = '';

	@state()
	private _isLoading = false;

	@state()
	private _exists = false;

	#context?: typeof UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context?.entityType, (entityType) => (this._entityType = entityType));
			this.observe(this.#context?.loading.isOn, (isLoading) => (this._isLoading = isLoading));
			this.observe(this.#context?.data, (data) => (this._exists = !!data));
		});
	}

	protected override render() {
		if (!this._exists && !this._isLoading) {
			return html`<umb-entity-detail-not-found entity-type=${this._entityType}></umb-entity-detail-not-found>`;
		}

		return html`<umb-workspace-editor ?loading=${this._isLoading} .backPath=${this.backPath}>
			<slot name="header" slot="header"></slot>
		</umb-workspace-editor>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-detail-workspace-editor': UmbEntityDetailWorkspaceEditorElement;
	}
}
