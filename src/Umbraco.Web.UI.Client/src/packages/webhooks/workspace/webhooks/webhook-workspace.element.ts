import { UmbWebhooksWorkspaceContext } from '../webhooks.context.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-webhook-workspace')
export class UmbWebhookWorkspaceElement extends UmbLitElement {
	#webhooksWorkspaceContext = new UmbWebhooksWorkspaceContext(this);

	render() {
		return html` <umb-workspace-editor
			headline="${this.localize.term('treeHeaders_webhooks')}"
			.enforceNoFooter=${true}>
		</umb-workspace-editor>`;
	}
}

export default UmbWebhookWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-workspace': UmbWebhookWorkspaceElement;
	}
}
