import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbEntityCreateOptionActionArgs<MetaArgsType> extends UmbEntityModel {
	meta: MetaArgsType;
}
