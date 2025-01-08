export enum TemporaryFileStatus {
	SUCCESS = 'success',
	WAITING = 'waiting',
	ERROR = 'error',
}

export interface UmbTemporaryFileModel {
	file: File;
	temporaryUnique: string;
	status?: TemporaryFileStatus;
}

export type UmbQueueHandlerCallback<TItem extends UmbTemporaryFileModel> = (item: TItem) => Promise<void>;

export type UmbUploadOptions<TItem extends UmbTemporaryFileModel> = {
	chunkSize?: number;
	callback?: UmbQueueHandlerCallback<TItem>;
};
