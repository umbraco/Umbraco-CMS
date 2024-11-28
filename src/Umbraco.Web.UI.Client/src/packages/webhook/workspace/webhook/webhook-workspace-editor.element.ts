import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
@customElement('umb-webhook-workspace-editor')
export class UmbWebhookWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`
			<umb-entity-detail-workspace-editor
				back-path="section/settings/workspace/webhook-root"></umb-entity-detail-workspace-editor>
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
