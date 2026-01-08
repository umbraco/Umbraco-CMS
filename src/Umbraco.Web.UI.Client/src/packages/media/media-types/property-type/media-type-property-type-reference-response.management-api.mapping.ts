import type { UmbMediaTypePropertyTypeReferenceModel } from './types.js';
import { UMB_MEDIA_TYPE_PROPERTY_TYPE_ENTITY_TYPE } from './entity.js';
import type { MediaTypePropertyTypeReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';

export class UmbMediaTypePropertyTypeReferenceResponseManagementApiDataMapping
	extends UmbControllerBase
	implements
		UmbDataSourceDataMapping<MediaTypePropertyTypeReferenceResponseModel, UmbMediaTypePropertyTypeReferenceModel>
{
	async map(data: MediaTypePropertyTypeReferenceResponseModel): Promise<UmbMediaTypePropertyTypeReferenceModel> {
		return {
			alias: data.alias!,
			mediaType: {
				alias: data.mediaType.alias!,
				icon: data.mediaType.icon!,
				name: data.mediaType.name!,
				unique: data.mediaType.id,
			},
			entityType: UMB_MEDIA_TYPE_PROPERTY_TYPE_ENTITY_TYPE,
			name: data.name!,
			unique: data.id,
		};
	}
}

export { UmbMediaTypePropertyTypeReferenceResponseManagementApiDataMapping as api };
