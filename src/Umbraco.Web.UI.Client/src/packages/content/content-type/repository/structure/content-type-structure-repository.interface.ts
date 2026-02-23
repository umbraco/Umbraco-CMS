import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbContentTypeStructureRepository<ItemType> {
	requestAllowedChildrenOf(
		unique: string,
		parentContentUnique: string | null,
	): Promise<UmbDataSourceResponse<UmbPagedModel<ItemType>>>;

	getAllowedParents?(unique: string): Promise<UmbDataSourceResponse<Array<UmbEntityModel>>>;
}
