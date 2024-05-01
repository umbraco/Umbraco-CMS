import type {
	UmbContentTypeCompositionCompatibleModel,
	UmbContentTypeCompositionReferenceModel,
	UmbContentTypeAvailableCompositionRequestModel,
} from './types.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeCompositionDataSource<
	CompositionReferenceModelType extends UmbContentTypeCompositionReferenceModel,
	CompositionCompatibleModelType extends UmbContentTypeCompositionCompatibleModel,
	AvailableCompositionsRequestType extends UmbContentTypeAvailableCompositionRequestModel,
> {
	getReferences(unique: string): Promise<UmbDataSourceResponse<Array<CompositionReferenceModelType>>>;
	availableCompositions(
		args: AvailableCompositionsRequestType,
	): Promise<UmbDataSourceResponse<Array<CompositionCompatibleModelType>>>;
}
