import { UMB_USER_COLLECTION_ALIAS } from '../collection/manifests.js';
import { UMB_USER_ENTITY_TYPE, UMB_USER_ROOT_ENTITY_TYPE } from '../entity.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbCollectionElement } from '@umbraco-cms/backoffice/collection';
import { UmbWorkspaceElement } from '@umbraco-cms/backoffice/workspace';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';

@customElement('umb-section-view-users')
export class UmbSectionViewUsersElement extends UmbLitElement {
	#routes: UmbRoute[] = [
		{
			path: 'collection',
			component: () => {
				const element = new UmbCollectionElement();
				const entityContext = new UmbEntityContext(element);
				entityContext.setEntityType(UMB_USER_ROOT_ENTITY_TYPE);
				entityContext.setUnique(null);
				element.setAttribute('alias', UMB_USER_COLLECTION_ALIAS);
				return element;
			},
		},
		{
			path: 'user',
			component: () => {
				const element = new UmbWorkspaceElement();
				element.setAttribute('entity-type', UMB_USER_ENTITY_TYPE);
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

	static override styles = [
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
