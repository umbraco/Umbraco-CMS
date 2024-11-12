import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../entity-detail-workspace.context-token.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-entity-detail-workspace-editor')
export class UmbEntityDetailWorkspaceEditorElement extends UmbLitElement {
	@property({ attribute: 'back-path' })
	public backPath?: string;

	@state()
	private _entityType?: string;

	@state()
	private _isLoading = false;

	@state()
	private _exists = false;

	@state()
	private _isNew? = false;

	#context?: typeof UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context?.entityType, (entityType) => (this._entityType = entityType));
			this.observe(this.#context?.loading.isOn, (isLoading) => (this._isLoading = isLoading));
			this.observe(this.#context?.data, (data) => (this._exists = !!data));
			this.observe(this.#context?.isNew, (isNew) => (this._isNew = isNew));
		});
	}

	protected override render() {
		if (!this._exists && !this._isLoading) {
			return html`<umb-entity-detail-not-found
				entity-type=${ifDefined(this._entityType)}></umb-entity-detail-not-found>`;
		}

		return html`<umb-workspace-editor ?loading=${this._isLoading} .backPath=${this.backPath}>
			<slot name="header" slot="header"></slot>
			${this.#renderEntityActions()}
			<slot></slot>
		</umb-workspace-editor>`;
	}

	#renderEntityActions() {
		if (this._isNew) return nothing;
		return html`<umb-workspace-entity-action-menu slot="action-menu"></umb-workspace-entity-action-menu>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-detail-workspace-editor': UmbEntityDetailWorkspaceEditorElement;
	}
}
