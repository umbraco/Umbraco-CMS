import type { MetaEntityBulkAction } from '../extension-registry/extensions/entity-bulk-action.extension.js';

export type * from './common/types.js';
export type * from './entity-bulk-action.interface.js';
export type * from './entity-bulk-action-element.interface.js';

export interface UmbEntityBulkActionArgs<MetaArgsType extends MetaEntityBulkAction> {
	entityType: string;
	meta: MetaArgsType;
}
