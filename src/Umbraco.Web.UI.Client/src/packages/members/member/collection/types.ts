import type { UmbMemberDetailModel } from '../types.js';

export interface UmbMemberCollectionFilterModel {
	skip?: number;
	take?: number;
	memberTypeId?: string;
	filter?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberCollectionModel extends UmbMemberDetailModel {}
