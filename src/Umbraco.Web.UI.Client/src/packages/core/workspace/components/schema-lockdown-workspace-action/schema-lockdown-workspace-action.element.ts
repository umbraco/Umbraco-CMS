import type { UmbSchemaLockdownWorkspaceActionApi } from './schema-lockdown-workspace-action.api.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-schema-lockdown-workspace-action')
export class UmbSchemaLockdownWorkspaceActionElement extends UmbLitElement {
	api: UmbSchemaLockdownWorkspaceActionApi | undefined;

	override render() {
		return html`
			<uui-button @click=${() => this.api?.execute()} compact>
				<uui-tag look="outline" color="default">
					<umb-localize key="schemaLockdown_headline"></umb-localize>
				</uui-tag>
			</uui-button>
		`;
	}

	static override styles = [
		css`
			uui-tag {
				text-wrap: nowrap;
			}
		`,
	];
}

export { UmbSchemaLockdownWorkspaceActionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-schema-lockdown-workspace-action': UmbSchemaLockdownWorkspaceActionElement;
	}
}
