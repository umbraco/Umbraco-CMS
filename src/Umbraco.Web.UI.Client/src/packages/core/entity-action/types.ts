import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type * from './common/types.js';
export type * from './default/types.js';
export type * from './entity-action-element.interface.js';
export type * from './entity-action.extension.js';
export type * from './entity-action.interface.js';

export interface UmbEntityActionArgs<MetaArgsType> extends UmbEntityModel {
	meta: MetaArgsType;
}
