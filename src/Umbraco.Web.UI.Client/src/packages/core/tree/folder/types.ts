import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbFolderModel extends UmbEntityModel {
	name: string;
}

export interface UmbCreateFolderModel {
	unique: string;
	parentUnique: string | null;
	parent: UmbEntityModel;
	name: string;
}

export interface UmbUpdateFolderModel {
	unique: string;
	name: string;
}
