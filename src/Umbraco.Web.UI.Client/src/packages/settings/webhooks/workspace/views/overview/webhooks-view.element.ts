import { UmbWebhooksWorkspaceContext, UMB_APP_WEBHOOKS_CONTEXT_TOKEN } from '../../webhooks.context.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-webhooks-overview-view')
export class UmbWebhooksOverviewViewElement extends UmbLitElement {

	#webhooksContext?: UmbWebhooksWorkspaceContext;
	constructor() {
		super();

		this.consumeContext(UMB_APP_WEBHOOKS_CONTEXT_TOKEN, (instance) => {
			this.#webhooksContext = instance;
		});
	}

	render() {
		return html`
			<div id="webhooks-layout">
				<table class="table table-hover" ng-if="!vm.loading">
					<thead>
						<tr>
						<th>Enabled</th>
						<th>Events</th>
						<th>Url</th>
						<th>Types</th>
						</tr>
					</thead>
					<tbody>
						
					</tbody>
				</table>
			</div>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}

			#webhooks-layout {
				padding-bottom: var(--uui-size-layout-1);
				display: grid;
				grid-template-columns: 7fr 2fr;
				grid-template-rows: auto auto;
				gap: 20px 20px;
				grid-auto-flow: row;
				grid-template-areas:
					'saved-searches info'
					'common-messages info';
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhooks-overview-view': UmbWebhooksOverviewViewElement;
	}
}
