// Read
export type {
	UmbReadDetailDataSource,
	UmbReadDetailDataSourceConstructor,
} from './read/read-detail-data-source.interface.js';
export type { UmbReadDetailRepository } from './read/read-detail-repository.interface.js';

// Combined
export type { UmbDetailDataSource, UmbDetailDataSourceConstructor } from './detail-data-source.interface.js';
export { UmbDetailRepositoryBase } from './detail-repository-base.js';
export type { UmbDetailRepository } from './detail-repository.interface.js';

export type * from './detail-repository.interface.js';
