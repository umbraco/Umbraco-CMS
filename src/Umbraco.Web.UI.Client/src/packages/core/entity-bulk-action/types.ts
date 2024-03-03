import type { MetaEntityBulkAction } from '../extension-registry/models/entity-bulk-action.model.js';

export interface UmbEntityBulkActionArgs<MetaArgsType extends MetaEntityBulkAction> {
	entityType: string;
	meta: MetaArgsType;
}
