import type { UmbTemplatingProductionModeWorkspaceActionApi } from './production-mode-workspace-action.api.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-templating-production-mode-workspace-action')
export class UmbTemplatingProductionModeWorkspaceActionElement extends UmbLitElement {
	api: UmbTemplatingProductionModeWorkspaceActionApi | undefined;

	override render() {
		return html`
			<uui-button @click=${() => this.api?.execute()} compact>
				<uui-tag look="outline" color="default">
					<umb-localize key="template_productionMode"></umb-localize>
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

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-production-mode-workspace-action': UmbTemplatingProductionModeWorkspaceActionElement;
	}
}
