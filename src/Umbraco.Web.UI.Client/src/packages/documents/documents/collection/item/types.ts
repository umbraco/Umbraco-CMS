import type { UmbDocumentEntityType } from '../../entity.js';
import type { UmbDocumentItemVariantModel } from '../../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbDocumentCollectionItemModel extends UmbEntityWithFlags {
	ancestors: Array<UmbEntityModel>;
	creator?: string | null;
	// TODO (V20): make `contentType` required when the deprecated `documentType` field is removed.
	contentType?: {
		unique: string;
		icon: string;
		alias: string;
		collection: UmbReferenceByUnique | null;
	};
	/**
	 * @deprecated Use `contentType` instead. This field will be removed in v20.
	 */
	documentType: {
		unique: string;
		icon: string;
		alias: string;
	};
	entityType: UmbDocumentEntityType;
	hasChildren?: boolean;
	isProtected: boolean;
	isTrashed: boolean;
	sortOrder: number;
	unique: string;
	updater?: string | null;
	values: Array<{ alias: string; culture?: string; segment?: string; value: string; editorAlias: string }>;
	variants: Array<UmbDocumentItemVariantModel>;
}

export interface UmbEditableDocumentCollectionItemModel {
	item: UmbDocumentCollectionItemModel;
	editPath: string;
}
