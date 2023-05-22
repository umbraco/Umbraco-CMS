import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement } from 'lit/decorators.js';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

import '../collection/views/table/user-collection-table-view.element';
import '../collection/views/grid/user-collection-grid-view.element';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-section-view-users')
export class UmbSectionViewUsersElement extends UmbLitElement {
	#routes: UmbRoute[] = [
		{
			path: 'collection',
			component: () => import('../collection/user-collection.element'),
		},
		{
			path: 'user',
			component: () => import('../workspace/user-workspace.element'),
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

export default UmbSectionViewUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-users': UmbSectionViewUsersElement;
	}
}
