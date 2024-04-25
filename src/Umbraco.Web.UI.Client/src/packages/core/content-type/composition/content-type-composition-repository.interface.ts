import type {
	UmbContentTypeCompositionCompatibleModel,
	UmbContentTypeCompositionReferenceModel,
	UmbContentTypeCompositionRequestModel,
} from './types.js';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeCompositionRepository {
	getReferences<ResponseType extends UmbContentTypeCompositionReferenceModel>(
		unique: string,
	): Promise<UmbRepositoryResponse<Array<ResponseType>>>;
	availableCompositions<
		ResponseType extends UmbContentTypeCompositionCompatibleModel,
		ArgsType extends UmbContentTypeCompositionRequestModel,
	>(
		args: ArgsType,
	): Promise<UmbRepositoryResponse<Array<ResponseType>>>;
}
