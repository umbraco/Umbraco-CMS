import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type {
	IReferenceResponseModelDefaultReferenceResponseModel as DefaultReferenceResponseModel,
	IReferenceResponseModelDocumentReferenceResponseModel as DocumentReferenceResponseModel,
	IReferenceResponseModelMediaReferenceResponseModel as MediaReferenceResponseModel,
	IReferenceResponseModelMemberReferenceResponseModel as MemberReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse, UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbReferenceItemModel extends UmbEntityModel {}

/**
 * The aggregate ("worst") publish state of a referenced element that is not fully published — see
 * {@link UmbReferencedElementWithPendingChangesModel}.
 */
export type UmbReferencedElementPendingChangesState = 'draft' | 'publishedPendingChanges';

/**
 * An element that the entity references (directly, via an Element Picker property or as embedded reusable block
 * content) and that is not fully published. `state` is the worst-wins aggregate across all variants; `isScheduled`
 * only says a future scheduled publish exists on some variant, not when.
 */
export interface UmbReferencedElementWithPendingChangesModel extends UmbReferenceItemModel {
	state: UmbReferencedElementPendingChangesState;
	isScheduled: boolean;
}

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

	requestReferencedElementsWithPendingChanges?(
		unique: string,
		skip?: number,
		take?: number,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbReferencedElementWithPendingChangesModel>>>;
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

	getReferencedElementsWithPendingChanges?(
		unique: string,
		skip?: number,
		take?: number,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferencedElementWithPendingChangesModel>>>;
}
