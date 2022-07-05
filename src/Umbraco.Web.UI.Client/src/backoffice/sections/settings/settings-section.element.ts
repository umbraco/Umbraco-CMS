import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';
import { UmbContextConsumerMixin } from '../../../core/context';

import './settings-section-tree.element';
@customElement('umb-settings-section')
export class UmbSettingsSection extends UmbContextConsumerMixin(LitElement) {
	// TODO: hardcoded tree routes. These should come from extensions
	@state()
	private _routes: Array<IRoute> = [
		{
			path: 'dashboard',
			component: () => import('../../components/section-dashboards.element'),
		},
		{
			path: 'extensions',
			component: () => import('../../editors/editor-extensions.element'),
		},
		// TODO: who should own this logic? Should it be each tree/editor that knows sub-routes?
		{
			path: 'data-type/:id',
			component: () => import('../../editors/editor-data-type.element'),
			setup(component: any, info: IRoutingInfo) {
				component.id = info.match.params.id;
			},
		},
		{
			path: '**',
			redirectTo: 'dashboard',
		},
	];

	render() {
		return html`
			<umb-section-layout>
				<umb-section-sidebar>
					<umb-settings-section-tree></umb-settings-section-tree>
				</umb-section-sidebar>

				<umb-section-main>
					<router-slot id="router-slot" .routes="${this._routes}"></router-slot>
				</umb-section-main>
			</umb-section-layout>
		`;
	}
}
