import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type {
	ElementItemResponseModel,
	IReferenceResponseModelDefaultReferenceResponseModel as DefaultReferenceResponseModel,
	IReferenceResponseModelDocumentReferenceResponseModel as DocumentReferenceResponseModel,
	IReferenceResponseModelMediaReferenceResponseModel as MediaReferenceResponseModel,
	IReferenceResponseModelMemberReferenceResponseModel as MemberReferenceResponseModel,
	PublishableVariantStateModel,
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

/**
 * Wire shape of the (not yet implemented) `GET /{document|element}/{id}/referenced-elements-with-pending-changes`
 * endpoint. Declared here rather than in the generated API client because there is no real backend for it yet —
 * the C# team is designing/implementing it separately. Once it exists and the client is regenerated via
 * `/umb-update-openapi`, replace these with the generated equivalents and delete this block.
 */
export interface UmbReferencedElementWithPendingChangesServerModel {
	element: ElementItemResponseModel;
	state: PublishableVariantStateModel;
	isScheduled: boolean;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPagedReferencedElementWithPendingChangesServerModel
	extends UmbPagedModel<UmbReferencedElementWithPendingChangesServerModel> {}

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
