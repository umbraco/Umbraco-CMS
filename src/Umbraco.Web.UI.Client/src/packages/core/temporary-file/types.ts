import type { TemporaryFileConfigurationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export enum TemporaryFileStatus {
	SUCCESS = 'success',
	WAITING = 'waiting',
	ERROR = 'error',
	CANCELLED = 'cancelled',
}

export interface UmbTemporaryFileModel {
	file: File;
	temporaryUnique: string;
	status?: TemporaryFileStatus;
	onProgress?: (progress: number) => void;
	/**
	 * The abort signal used to cancel the upload.
	 * @deprecated Use {@link abortController} instead.
	 */
	abortSignal?: AbortSignal;
	/**
	 * The abort controller used to cancel the upload.
	 */
	abortController?: AbortController;
}

export type UmbQueueHandlerCallback<TItem extends UmbTemporaryFileModel> = (item: TItem) => Promise<void>;

export type UmbUploadOptions<TItem extends UmbTemporaryFileModel> = {
	chunkSize?: number;
	callback?: UmbQueueHandlerCallback<TItem>;
};

export type UmbTemporaryFileConfigurationModel = TemporaryFileConfigurationResponseModel;
