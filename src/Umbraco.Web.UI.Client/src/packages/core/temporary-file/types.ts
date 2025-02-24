import type { TemporaryFileConfigurationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export enum TemporaryFileStatus {
	SUCCESS = 'success',
	WAITING = 'waiting',
	ERROR = 'error',
}

export interface UmbTemporaryFileModel {
	file: File;
	temporaryUnique: string;
	status?: TemporaryFileStatus;
	onProgress?: (progress: number) => void;
	abortSignal?: AbortSignal;
}

export type UmbQueueHandlerCallback<TItem extends UmbTemporaryFileModel> = (item: TItem) => Promise<void>;

export type UmbUploadOptions<TItem extends UmbTemporaryFileModel> = {
	chunkSize?: number;
	callback?: UmbQueueHandlerCallback<TItem>;
};

export type UmbTemporaryFileConfigurationModel = TemporaryFileConfigurationResponseModel;
