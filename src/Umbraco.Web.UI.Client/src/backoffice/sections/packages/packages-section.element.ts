import '../../../backoffice/editors/shared/editor-entity/editor-entity.element';

import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';

@customElement('umb-packages-section')
export class UmbPackagesSection extends UmbContextConsumerMixin(LitElement) {
	private umbExtensionRegistry?: UmbExtensionRegistry;

	private _routes: IRoute[] = [
		{
			path: 'details/:id',
			component: () => import('./packages-details.element'),
			setup(component: any, info: IRoutingInfo) {
				component.id = info.match.params.id;
			},
		},
		{
			path: '',
			component: () => import('./packages-editor.element'),
		},
		{
			path: '**',
			redirectTo: '',
		},
	];

	render() {
		return html` <router-slot .routes=${this._routes}></router-slot> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-section': UmbPackagesSection;
	}
}
