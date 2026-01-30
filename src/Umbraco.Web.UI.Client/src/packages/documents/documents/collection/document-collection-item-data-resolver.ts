import { UmbDocumentItemDataResolver } from '../item/document-item-data-resolver.js';
import type { UmbDocumentCollectionItemModel, UmbDocumentCollectionItemPropertyValueModel } from './types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';

export class UmbDocumentCollectionItemDataResolver
	extends UmbDocumentItemDataResolver<UmbDocumentCollectionItemModel>
	implements UmbItemDataResolver
{
	getPropertyByAlias(alias: string): UmbDocumentCollectionItemPropertyValueModel | undefined {
		const item = this.getData();
		const culture = this.getCulture();
		const prop = item?.values.find((x) => x.alias === alias && (!x.culture || x.culture === culture));
		return prop;
	}

	getSystemValue(alias: string): string | number | null | undefined {
		const item = this.getData();
		switch (alias) {
			case 'contentTypeAlias':
				return item?.documentType.alias;

			case 'createDate': {
				const variant = this._getCurrentVariant();
				return variant?.createDate?.toLocaleString();
			}

			case 'creator':
			case 'owner':
				return item?.creator;

			case 'published': {
				const variant = this._getCurrentVariant();
				return variant?.state !== DocumentVariantStateModel.DRAFT ? 'True' : 'False';
			}

			case 'sortOrder':
				return item?.sortOrder;

			case 'updateDate': {
				const variant = this._getCurrentVariant();
				return variant?.updateDate?.toLocaleString();
			}
			case 'updater':
				return item?.updater;

			default:
				return null;
		}
	}
}
