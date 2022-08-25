import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute } from 'router-slot';

import '../shared/section-trees.element.ts';

@customElement('umb-section-members')
export class UmbSectionMembers extends LitElement {
	// TODO: make this code reusable across sections
	@state()
	private _routes: Array<IRoute> = [
		{
			path: 'dashboard',
			component: () => import('../shared/section-dashboards.element'),
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
					<umb-section-trees></umb-section-trees>
				</umb-section-sidebar>
				<umb-section-main>
					<router-slot id="router-slot" .routes="${this._routes}"></router-slot>
				</umb-section-main>
			</umb-section-layout>
		`;
	}
}

export default UmbSectionMembers;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-members': UmbSectionMembers;
	}
}
