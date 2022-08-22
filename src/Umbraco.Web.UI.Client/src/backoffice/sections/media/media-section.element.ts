import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';

import './media-section-tree.element';

@customElement('umb-media-section')
export class UmbMediaSection extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host,
			#router-slot {
				display: flex;
				width: 100%;
				height: 100%;
			}
		`,
	];

	@state()
	private _routes: Array<IRoute> = [
		{
			path: 'dashboard',
			component: () => import('../shared/section-dashboards.element'),
			setup: () => {
				this._currentNodeId = undefined;
			},
		},
		{
			path: 'node/:nodeId',
			component: () => import('../../editors/media/editor-media.element'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				this._currentNodeId = info.match.params.nodeId;
				component.id = this._currentNodeId;
			},
		},
		{
			path: '**',
			redirectTo: 'dashboard',
		},
	];

	@state()
	private _currentNodeId?: string;

	render() {
		return html`
			<umb-section-sidebar>
				<umb-media-section-tree .currentNodeId="${this._currentNodeId}"></umb-media-section-tree>
			</umb-section-sidebar>
			<router-slot id="router-slot" .routes="${this._routes}"></router-slot>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-section': UmbMediaSection;
	}
}
