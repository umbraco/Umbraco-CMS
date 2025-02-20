import type { UmbRelationEntityType } from './entity.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type {
	DefaultReferenceResponseModel,
	DocumentReferenceResponseModel,
	MediaReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbRelationDetailModel {
	unique: string;
	entityType: UmbRelationEntityType;
	relationType: {
		unique: string;
	};
	parent: {
		unique: string;
		name: string;
	};
	child: {
		unique: string;
		name: string;
	};
	createDate: string;
	comment: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbReferenceItemModel extends UmbEntityModel {}

export type UmbReferenceModel =
	| DefaultReferenceResponseModel
	| DocumentReferenceResponseModel
	| MediaReferenceResponseModel;
