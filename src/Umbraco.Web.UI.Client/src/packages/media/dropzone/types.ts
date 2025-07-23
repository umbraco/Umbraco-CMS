import type { UmbFileDropzoneItemStatus } from './constants.js';
import type { UUIFileFolder } from '@umbraco-cms/backoffice/external/uui';
import type { UmbTemporaryFileModel } from '@umbraco-cms/backoffice/temporary-file';

export interface UmbFileDropzoneDroppedItems {
	files: Array<File>;
	folders: Array<UUIFileFolder>;
}

export interface UmbUploadableItem {
	unique: string;
	parentUnique: string | null;
	status: UmbFileDropzoneItemStatus;
	progress: number;
	folder?: { name: string };
	temporaryFile?: UmbTemporaryFileModel;
}

export interface UmbUploadableFile extends UmbUploadableItem {
	temporaryFile: UmbTemporaryFileModel;
}

export interface UmbUploadableFolder extends UmbUploadableItem {
	folder: { name: string };
}

export interface UmbFileDropzoneProgress {
	total: number;
	completed: number;
}
