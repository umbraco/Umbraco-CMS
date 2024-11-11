import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export interface UmbFolderModel extends UmbEntityModel {
	name: string;
}

export interface MetaEntityActionFolderKind extends MetaEntityActionDefaultKind {
	folderRepositoryAlias: string;
}
