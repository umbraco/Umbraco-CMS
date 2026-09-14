import { UmbMediaCollectionRepository } from '../collection/repository/index.js';
import { UMB_MEDIA_COLLECTION_ALIAS } from '../constants.js';
import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbCollectionElement } from '@umbraco-cms/backoffice/collection';
import { umbMapDataTypeToCollectionConfiguration } from '@umbraco-cms/backoffice/content';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-media-dashboard')
export class UmbMediaDashboardElement extends UmbLitElement {
	#dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);
	#entityContext = new UmbEntityContext(this);
	#mediaCollectionRepository = new UmbMediaCollectionRepository(this);

	@state()
	private _routes?: UmbRoute[];

	constructor() {
		super();

		this.#defineRoutes();

		this.#entityContext.setEntityType(UMB_MEDIA_ROOT_ENTITY_TYPE);
		this.#entityContext.setUnique(null);
	}

	async #defineRoutes() {
		const config = await this.#mediaCollectionRepository.getDefaultConfiguration();

		await this.#dataTypeDetailRepository.requestByUnique(config.defaultDataTypeId);

		this.observe(
			await this.#dataTypeDetailRepository.byUnique(config.defaultDataTypeId),
			(dataType) => {
				if (!dataType) return;

				const dataTypeConfig: UmbCollectionConfiguration = {
					unique: '',
					...umbMapDataTypeToCollectionConfiguration(dataType),
				};

				this._routes = [
					{
						path: 'collection',
						component: () => {
							const element = new UmbCollectionElement();
							element.alias = UMB_MEDIA_COLLECTION_ALIAS;
							element.config = dataTypeConfig;
							return element;
						},
					},
					{
						path: '',
						pathMatch: 'full',
						redirectTo: 'collection',
					},
					{
						path: `**`,
						component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
					},
				];
			},
			'_observeConfigDataType',
		);
	}

	override render() {
		if (!this._routes) return;
		return html`<umb-router-slot id="router-slot" .routes=${this._routes}></umb-router-slot>`;
	}

	static override styles = [
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

export default UmbMediaDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-dashboard': UmbMediaDashboardElement;
	}
}
