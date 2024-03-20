import { UmbWebhookWorkspaceContext } from '../webhook.context.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDefaultWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-webhook-workspace')
export class UmbWebhookWorkspaceElement extends UmbLitElement {
	/**
	 * The context for the webhook workspace.
	 * This is used to provide the workspace with the necessary data and services even though it does not have any references.
	 */
	#webhookWorkspaceContext = new UmbWebhookWorkspaceContext(this);

	render() {
		return html` <umb-workspace-editor
			headline="${this.localize.term('treeHeaders_webhooks')}"
			.enforceNoFooter=${true}>
		</umb-workspace-editor>`;
	}
}

export { UmbWebhookWorkspaceElement as element };

export { UmbDefaultWorkspaceContext as api };
declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-workspace': UmbWebhookWorkspaceElement;
	}
}
