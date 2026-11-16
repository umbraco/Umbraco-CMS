import type { UmbDocumentBlueprintFolderEntityType } from '../../../../entity.js';
import type { UmbEntityFlag, UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';

export interface UmbDocumentBlueprintFolderItemModel extends UmbEntityWithFlags {
	entityType: UmbDocumentBlueprintFolderEntityType;
	name: string;
	unique: string;
	icon: string;
	flags: Array<UmbEntityFlag>;
}
