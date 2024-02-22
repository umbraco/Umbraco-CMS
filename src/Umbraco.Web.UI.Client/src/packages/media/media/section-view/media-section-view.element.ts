import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';
import type { UmbCollectionBulkActionPermissions, UmbCollectionConfiguration } from '../../../core/collection/types.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbCollectionElement } from '@umbraco-cms/backoffice/collection';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-media-section-view')
export class UmbMediaSectionViewElement extends UmbLitElement {
	// TODO: [LK] Check if we can get the default data-type ID (for the Media ListView) from the server.
	//private readonly defaultDataTypeId: string = '3a0156c4-3b8c-4803-bdc1-6871faa83fff';
	private readonly defaultDataTypeUnique: string = 'dt-collectionView';

	#dataTypeUnique: string = this.defaultDataTypeUnique;

	#dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);

	@state()
	private _routes?: UmbRoute[];

	constructor() {
		super();

		this.#defineRoutes();
	}

	async #defineRoutes() {
		await this.#dataTypeDetailRepository.requestByUnique(this.#dataTypeUnique);

		this.observe(
			await this.#dataTypeDetailRepository.byUnique(this.#dataTypeUnique),
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
			dataTypeId: dataType.unique,
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
