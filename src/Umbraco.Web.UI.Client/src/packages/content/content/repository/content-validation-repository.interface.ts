import type { UmbContentDetailModel } from '../types.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbContentValidationRepository<DetailModelType extends UmbContentDetailModel> {
	validateCreate(model: DetailModelType, parentUnique: string | null): Promise<UmbRepositoryResponse<string>>;
	validateSave(model: DetailModelType, variantIds: Array<UmbVariantId>): Promise<UmbRepositoryResponse<string>>;
}
