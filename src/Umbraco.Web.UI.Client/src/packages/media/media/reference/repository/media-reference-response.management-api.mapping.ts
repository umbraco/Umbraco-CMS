import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaReferenceModel } from './types.js';
import type { MediaReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbDataMapping } from '@umbraco-cms/backoffice/repository';

export class UmbMediaReferenceResponseManagementApiDataMapping
	extends UmbControllerBase
	implements UmbDataMapping<MediaReferenceResponseModel, UmbMediaReferenceModel>
{
	async map(data: MediaReferenceResponseModel): Promise<UmbMediaReferenceModel> {
		return {
			entityType: UMB_MEDIA_ENTITY_TYPE,
			id: data.id,
			mediaType: {
				alias: data.mediaType.alias,
				icon: data.mediaType.icon,
				name: data.mediaType.name,
			},
			name: data.name,
			// TODO: this is a hardcoded array until the server can return the correct variants array
			variants: [
				{
					culture: null,
					name: data.name ?? '',
				},
			],
			unique: data.id,
		};
	}
}

export { UmbMediaReferenceResponseManagementApiDataMapping as api };
