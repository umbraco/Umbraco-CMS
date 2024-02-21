import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbCollectionElement } from '@umbraco-cms/backoffice/collection';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-media-section-view')
export class UmbMediaSectionViewElement extends UmbLitElement {
	#routes: UmbRoute[] = [
		{
			path: 'collection',
			component: () => {

				// TODO: [LK] Work-in-progress. Need to get the data-type configuration for the Media Collection.
				const config = {
					unique: '',
					dataTypeId: '', //'3a0156c4-3b8c-4803-bdc1-6871faa83fff', //'dt-collectionView',
					allowedEntityBulkActions: {
						allowBulkCopy: true,
						allowBulkDelete: true,
						allowBulkMove: true,
						allowBulkPublish: false,
						allowBulkUnpublish: false,
					},
					orderBy: 'updateDate',
					orderDirection: 'asc',
					pageSize: 50,
					useInfiniteEditor: false,
					userDefinedProperties: undefined,
				};

				const element = new UmbCollectionElement();
				element.alias = UMB_MEDIA_COLLECTION_ALIAS;
				element.config = config;
				return element;
			},
		},
		{
			path: '',
			redirectTo: 'collection',
		},
	];

	render() {
		return html`<umb-router-slot id="router-slot" .routes=${this.#routes}></umb-router-slot>`;
	}

	static styles = [
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

export default UmbMediaSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-section-view': UmbMediaSectionViewElement;
	}
}
