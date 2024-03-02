import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';
import { UmbMediaCollectionRepository } from '../collection/repository/index.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbCollectionElement } from '@umbraco-cms/backoffice/collection';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import type {
	UmbCollectionBulkActionPermissions,
	UmbCollectionConfiguration,
} from '@umbraco-cms/backoffice/collection';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-media-section-view')
export class UmbMediaSectionViewElement extends UmbLitElement {
	#dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);
	#mediaCollectionRepository = new UmbMediaCollectionRepository(this);

	@state()
	private _routes?: UmbRoute[];

	constructor() {
		super();

		this.#defineRoutes();
	}

	async #defineRoutes() {
		const config = await this.#mediaCollectionRepository.getDefaultConfiguration();

		await this.#dataTypeDetailRepository.requestByUnique(config.defaultDataTypeId);

		this.observe(
			await this.#dataTypeDetailRepository.byUnique(config.defaultDataTypeId),
			(dataType) => {
				if (!dataType) return;

				const dataTypeConfig = this.#mapDataTypeConfigToCollectionConfig(dataType);

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
						redirectTo: 'collection',
					},
				];
			},
			'_observeConfigDataType',
		);
	}

	#mapDataTypeConfigToCollectionConfig(dataType: UmbDataTypeDetailModel): UmbCollectionConfiguration {
		const config = new UmbPropertyEditorConfigCollection(dataType.values);
		return {
			unique: '',
			dataTypeId: '',
			allowedEntityBulkActions: config?.getValueByAlias<UmbCollectionBulkActionPermissions>('bulkActionPermissions'),
			orderBy: config?.getValueByAlias('orderBy') ?? 'updateDate',
			orderDirection: config?.getValueByAlias('orderDirection') ?? 'asc',
			pageSize: Number(config?.getValueByAlias('pageSize')) ?? 50,
			useInfiniteEditor: config?.getValueByAlias('useInfiniteEditor') ?? false,
			userDefinedProperties: config?.getValueByAlias('includeProperties'),
		};
	}

	render() {
		if (!this._routes) return;
		return html`<umb-router-slot id="router-slot" .routes=${this._routes}></umb-router-slot>`;
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
