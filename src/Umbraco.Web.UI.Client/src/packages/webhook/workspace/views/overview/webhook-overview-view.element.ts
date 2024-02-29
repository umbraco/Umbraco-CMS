import { UMB_WEBHOOK_COLLECTION_ALIAS } from '../../../collection/index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-webook-root-workspace')
export class UmbWebhookRootWorkspaceElement extends UmbLitElement {
	render() {
		return html` <umb-collection alias=${UMB_WEBHOOK_COLLECTION_ALIAS}></umb-collection>`;
	}
}

export default UmbWebhookRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-root-workspace': UmbWebhookRootWorkspaceElement;
	}
}
