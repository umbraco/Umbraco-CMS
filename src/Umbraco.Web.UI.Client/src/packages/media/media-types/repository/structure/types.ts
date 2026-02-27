import type { UmbContentTypeStructureDataSource } from '@umbraco-cms/backoffice/content-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbAllowedMediaTypeModel extends UmbEntityModel {
	name: string;
	description: string | null;
	icon: string | null;
}

export interface UmbMediaTypeStructureDataSource extends UmbContentTypeStructureDataSource<UmbAllowedMediaTypeModel> {
	getAllowedParentsOf(unique: string): Promise<UmbDataSourceResponse<Array<UmbEntityModel>>>;
}
