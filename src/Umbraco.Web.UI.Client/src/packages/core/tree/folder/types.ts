import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export type * from './entity-action/types.js';
export type * from './entity-create-option-action/types.js';

export interface UmbFolderModel extends UmbEntityModel {
	name: string;
}

export interface MetaEntityActionFolderKind extends MetaEntityActionDefaultKind {
	folderRepositoryAlias: string;
}
