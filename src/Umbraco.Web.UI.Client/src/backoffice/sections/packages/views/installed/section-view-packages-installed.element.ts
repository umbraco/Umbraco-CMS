import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';
import { umbHistoryService } from 'src/core/services/history';
import { UmbEditorEntityElement } from '../../../../editors/shared/editor-entity/editor-entity.element';

@customElement('umb-section-view-packages-installed')
export class UmbSectionViewPackagesInstalledElement extends LitElement {
	@state()
	private _routes: IRoute[] = [
		{
			path: 'overview',
			component: () => import('./packages-installed-overview.element'),
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
			redirectTo: '/section/packages/view/installed/overview', //TODO: this should be dynamic
		},
	];

	constructor() {
		super();
		umbHistoryService.push({ label: ['Packages', 'Installed'], path: 'section/packages/view/installed/overview' });
	}

	render() {
		return html`<router-slot .routes=${this._routes}></router-slot>`;
	}
}

export default UmbSectionViewPackagesInstalledElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-packages-installed': UmbSectionViewPackagesInstalledElement;
	}
}
