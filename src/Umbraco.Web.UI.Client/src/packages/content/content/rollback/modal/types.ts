import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbContentRollbackModalData {
	rollbackRepositoryAlias: string;
	detailRepositoryAlias: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentRollbackModalValue extends UmbEntityModel {}
