import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbEntityActionArgs<MetaArgsType> extends UmbEntityModel {
	meta: MetaArgsType;
}
