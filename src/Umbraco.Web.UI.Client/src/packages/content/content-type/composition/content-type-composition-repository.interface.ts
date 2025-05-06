import type {
	UmbContentTypeCompositionCompatibleModel,
	UmbContentTypeCompositionReferenceModel,
	UmbContentTypeAvailableCompositionRequestModel,
} from './types.js';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeCompositionRepository<
	CompositionReferenceModelType extends
		UmbContentTypeCompositionReferenceModel = UmbContentTypeCompositionReferenceModel,
	CompositionCompatibleModelType extends
		UmbContentTypeCompositionCompatibleModel = UmbContentTypeCompositionCompatibleModel,
	AvailableCompositionsRequestType extends
		UmbContentTypeAvailableCompositionRequestModel = UmbContentTypeAvailableCompositionRequestModel,
> {
	getReferences(unique: string): Promise<UmbRepositoryResponse<Array<CompositionReferenceModelType>>>;
	availableCompositions(
		args: AvailableCompositionsRequestType,
	): Promise<UmbRepositoryResponse<Array<CompositionCompatibleModelType>>>;
}
