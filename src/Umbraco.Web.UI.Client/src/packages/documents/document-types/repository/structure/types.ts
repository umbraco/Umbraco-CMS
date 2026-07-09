import type { UmbContentTypeStructureDataSource } from '@umbraco-cms/backoffice/content-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbAllowedDocumentTypeModel extends UmbEntityModel {
	name: string;
	description: string | null;
	icon: string | null;
}

export interface UmbDocumentTypeStructureDataSource
	extends UmbContentTypeStructureDataSource<UmbAllowedDocumentTypeModel> {
	getAllowedParentsOf(unique: string): Promise<UmbDataSourceResponse<Array<UmbEntityModel>>>;
}
