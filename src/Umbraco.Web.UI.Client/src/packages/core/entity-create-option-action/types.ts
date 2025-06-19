import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type * from './entity-create-option-action.extension.js';
export type * from './entity-create-option-action.interface.js';

export interface UmbEntityCreateOptionActionArgs<MetaArgsType> extends UmbEntityModel {
	meta: MetaArgsType;
}
