import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-user-groups-section-view')
export class UmbUserGroupsSectionViewElement extends UmbLitElement {
	#routes: UmbRoute[] = [
		{
			path: 'collection',
			component: () => import('../collection/user-group-collection.element'),
		},
		{
			path: 'user-group',
			component: () => import('../workspace/user-group-workspace.element'),
		},
		{
			path: '**',
			redirectTo: 'collection',
		},
	];

	render() {
		return html`<umb-router-slot id="router-slot" .routes=${this.#routes}></umb-router-slot>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
			}

			#router-slot {
				height: calc(100% - var(--umb-header-layout-height));
			}
		`,
	];
}

export default UmbUserGroupsSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-groups-section-view': UmbUserGroupsSectionViewElement;
	}
}
