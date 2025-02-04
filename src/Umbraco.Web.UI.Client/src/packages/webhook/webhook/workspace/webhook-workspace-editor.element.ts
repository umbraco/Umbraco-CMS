import type { UmbWebhookDetailModel } from '../../types.js';
import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from './webhook-workspace.context-token.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
@customElement('umb-webhook-workspace-editor')
export class UmbWebhookWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_WEBHOOK_WORKSPACE_CONTEXT.TYPE;

	@state()
	_isNew?: boolean;

	@state()
	_url?: UmbWebhookDetailModel['url'];

	constructor() {
		super();

		this.consumeContext(UMB_WEBHOOK_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.observe(this.#workspaceContext.isNew, (isNew) => (this._isNew = isNew));
			this.observe(this.#workspaceContext.url, (url) => (this._url = url));
		});
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor back-path="section/settings/workspace/webhook-root">
				${this._isNew ? html`<h3 slot="header">Add Webhook</h3>` : html`<h3 slot="header">${this._url}</h3> `}
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbWebhookWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-workspace-editor': UmbWebhookWorkspaceEditorElement;
	}
}
