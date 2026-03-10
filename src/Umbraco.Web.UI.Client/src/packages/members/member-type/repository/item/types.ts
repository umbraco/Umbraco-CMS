import type { UmbMemberTypeEntityType, UmbMemberTypeFolderEntityType } from '../../entity.js';

export interface UmbMemberTypeItemModel {
	entityType: UmbMemberTypeEntityType | UmbMemberTypeFolderEntityType;
	unique: string;
	name: string;
	icon: string;
}
