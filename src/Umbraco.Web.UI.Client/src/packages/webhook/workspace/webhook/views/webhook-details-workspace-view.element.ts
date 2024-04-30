import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '@umbraco-cms/backoffice/culture';

@customElement('umb-webhook-details-workspace-view')
export class UmbWebhookDetailsWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	render() {
		return html`EDIT NPW `;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbWebhookDetailsWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-details-workspace-view': UmbWebhookDetailsWorkspaceViewElement;
	}
}
