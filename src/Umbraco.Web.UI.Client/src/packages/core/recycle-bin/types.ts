export type * from './conditions/types.js';
export type * from './entity-action/types.js';
export type * from './entity-bulk-action/types.js';

export type { UmbRecycleBinDataSource } from './recycle-bin-data-source.interface.js';
export type { UmbRecycleBinRepository } from './recycle-bin-repository.interface.js';

export interface UmbRecycleBinRestoreRequestArgs {
	unique: string;
	destination: {
		unique: string | null;
	};
}

export interface UmbRecycleBinTrashRequestArgs {
	unique: string;
}

export interface UmbRecycleBinOriginalParentRequestArgs {
	unique: string;
}
