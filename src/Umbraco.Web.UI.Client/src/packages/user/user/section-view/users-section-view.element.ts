import { UMB_USER_COLLECTION_ALIAS } from '../collection/manifests.js';
import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-section-view-users')
export class UmbSectionViewUsersElement extends UmbLitElement {
	#routes: UmbRoute[] = [
		{
			path: 'collection',
			component: () => {
				const element = document.createElement('umb-collection');
				element.setAttribute('alias', UMB_USER_COLLECTION_ALIAS);
				return element;
			},
		},
		{
			path: 'user',
			component: () => {
				const element = document.createElement('umb-workspace');
				element.setAttribute('entityType', UMB_USER_ENTITY_TYPE);
				return element;
			},
		},
		{
			path: '',
			redirectTo: 'collection',
		},
	];

	render() {
		return html` <umb-router-slot id="router-slot" .routes=${this.#routes}></umb-router-slot> `;
	}

	static styles = [
		UmbTextStyles,
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
