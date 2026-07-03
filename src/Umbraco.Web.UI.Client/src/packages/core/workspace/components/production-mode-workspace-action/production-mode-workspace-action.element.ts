import type { UmbProductionModeWorkspaceActionApi } from './production-mode-workspace-action.api.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-production-mode-workspace-action')
export class UmbProductionModeWorkspaceActionElement extends UmbLitElement {
	api: UmbProductionModeWorkspaceActionApi | undefined;

	override render() {
		return html`
			<uui-button @click=${() => this.api?.execute()} compact>
				<uui-tag look="outline" color="default">
					<umb-localize key="general_productionMode"></umb-localize>
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

export { UmbProductionModeWorkspaceActionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-production-mode-workspace-action': UmbProductionModeWorkspaceActionElement;
	}
}
