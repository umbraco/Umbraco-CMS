import type { UmbRelationEntityType } from './entity.js';
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

export type UmbReferenceModel =
	| DefaultReferenceResponseModel
	| DocumentReferenceResponseModel
	| MediaReferenceResponseModel;
