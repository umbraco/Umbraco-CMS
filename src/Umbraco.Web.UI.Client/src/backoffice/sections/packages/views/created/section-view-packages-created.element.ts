import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';
import { UmbEditorEntityElement } from '../../../../editors/shared/editor-entity/editor-entity.element';

@customElement('umb-section-view-packages-created')
export class UmbSectionViewPackagesCreatedElement extends LitElement {
	@state()
	private _routes: IRoute[] = [
		{
			path: 'overview',
			component: () => import('./packages-created-overview.element'),
		},
		{
			path: `:entityType/:key`,
			component: () => import('../../../../editors/shared/editor-entity/editor-entity.element'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const element = component as UmbEditorEntityElement;
				element.entityKey = info.match.params.key;
				element.entityType = info.match.params.entityType;
			},
		},
		{
			path: '**',
			redirectTo: '/section/packages/view/created/overview', //TODO: this should be dynamic
		},
	];

	render() {
		return html`<router-slot .routes=${this._routes}></router-slot>`;
	}
}

export default UmbSectionViewPackagesCreatedElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-packages-created': UmbSectionViewPackagesCreatedElement;
	}
}
