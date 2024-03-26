import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-webhook-workspace')
export class UmbWebhookWorkspaceElement extends UmbLitElement {
	render() {
		return html` <umb-workspace-editor
			headline="${this.localize.term('treeHeaders_webhooks')}"
			.enforceNoFooter=${true}>
		</umb-workspace-editor>`;
	}
}

export { UmbWebhookWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-workspace': UmbWebhookWorkspaceElement;
	}
}
