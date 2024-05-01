import type { UmbContentTypeStructureRepository } from '@umbraco-cms/backoffice/content-type';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeUploadableStructureRepository<ItemUploadableType>
	extends UmbContentTypeStructureRepository<ItemUploadableType> {
	requestAllowedMediaTypesOf(unique: string): Promise<UmbDataSourceResponse<ItemUploadableType>>;
}
