import type {
	UmbContentTypeStructureDataSource,
	UmbContentTypeStructureDataSourceConstructor,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeUploadableStructureDataSourceConstructor<ItemUploadableType>
	extends UmbContentTypeStructureDataSourceConstructor<ItemUploadableType> {
	new (host: UmbControllerHost): UmbContentTypeUploadableStructureDataSource<ItemUploadableType>;
}

export interface UmbContentTypeUploadableStructureDataSource<ItemUploadableType>
	extends UmbContentTypeStructureDataSource<ItemUploadableType> {
	getAllowedMediaTypesOf(fileExtension: string | null): Promise<UmbDataSourceResponse<ItemUploadableType>>;
}
