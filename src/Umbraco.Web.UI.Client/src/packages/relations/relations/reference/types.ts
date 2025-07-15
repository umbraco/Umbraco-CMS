import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type {
	DefaultReferenceResponseModel,
	DocumentReferenceResponseModel,
	MediaReferenceResponseModel,
	MemberReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse, UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbReferenceItemModel extends UmbEntityModel {}

export type UmbReferenceModel =
	| DefaultReferenceResponseModel
	| DocumentReferenceResponseModel
	| MediaReferenceResponseModel
	| MemberReferenceResponseModel;

export interface UmbEntityReferenceRepository extends UmbApi {
	requestReferencedBy(
		unique: string,
		skip?: number,
		take?: number,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbReferenceItemModel>>>;

	requestAreReferenced(
		uniques: Array<string>,
		skip?: number,
		take?: number,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbEntityModel>>>;

	requestDescendantsWithReferences?(
		unique: string,
		skip?: number,
		take?: number,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbEntityModel>>>;
}

export interface UmbEntityReferenceDataSource {
	getReferencedBy(
		unique: string,
		skip?: number,
		take?: number,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>>;

	getAreReferenced(
		uniques: Array<string>,
		skip?: number,
		take?: number,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>>;

	getReferencedDescendants?(
		unique: string,
		skip?: number,
		take?: number,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>>;
}
