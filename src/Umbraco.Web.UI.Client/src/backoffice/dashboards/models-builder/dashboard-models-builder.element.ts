import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbHistoryService } from 'src/core/services/history';

@customElement('umb-dashboard-models-builder')
export class UmbDashboardModelsBuilderElement extends LitElement {
	static styles = [UUITextStyles, css``];

	constructor() {
		super();
		umbHistoryService.push({
			label: ['Settings', 'Models Builder'],
			path: 'section/settings/dashboard/models-builder',
		});
	}

	render() {
		return html`
			<uui-box>
				<h1>Models Builder</h1>
			</uui-box>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-models-builder': UmbDashboardModelsBuilderElement;
	}
}
