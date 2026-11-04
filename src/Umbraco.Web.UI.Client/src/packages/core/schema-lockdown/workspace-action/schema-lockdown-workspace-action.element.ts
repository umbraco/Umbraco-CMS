import type { UmbSchemaLockdownWorkspaceActionApi } from './schema-lockdown-workspace-action.api.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-schema-lockdown-workspace-action')
export class UmbSchemaLockdownWorkspaceActionElement extends UmbLitElement {
	api: UmbSchemaLockdownWorkspaceActionApi | undefined;

	override render() {
		return html`
			<uui-button compact label=${this.localize.term('schemaLockdown_locked')} @click=${() => this.api?.execute()}>
				<uui-tag look="outline" color="default">
					<uui-icon name="icon-lock"></uui-icon>
					${this.localize.term('schemaLockdown_locked')}
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
