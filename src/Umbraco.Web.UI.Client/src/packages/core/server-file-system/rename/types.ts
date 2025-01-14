import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbRenameServerFileRepository<DetailType extends UmbEntityModel> {
	rename(unique: string, name: string): Promise<UmbDataSourceResponse<DetailType>>;
}

export interface UmbRenameServerFileDataSourceConstructor<DetailType extends UmbEntityModel> {
	new (host: UmbControllerHost): UmbRenameServerFileDataSource<DetailType>;
}

export interface UmbRenameServerFileDataSource<DetailType extends UmbEntityModel> {
	rename(unique: string, name: string): Promise<UmbDataSourceResponse<DetailType>>;
}

export interface ManifestEntityActionRenameServerFileKind
	extends ManifestEntityAction<MetaEntityActionRenameServerFileKind> {
	type: 'entityAction';
	kind: 'renameServerFile';
}

export interface MetaEntityActionRenameServerFileKind extends MetaEntityActionDefaultKind {
	renameRepositoryAlias: string;
	itemRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbRenameFileServerFileEntityActionKind: ManifestEntityActionRenameServerFileKind;
	}
}
