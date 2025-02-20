import type { MetaEntityBulkAction } from '../extension-registry/extensions/entity-bulk-action.extension.js';

export interface UmbEntityBulkActionArgs<MetaArgsType extends MetaEntityBulkAction> {
	entityType: string;
	meta: MetaArgsType;
}
