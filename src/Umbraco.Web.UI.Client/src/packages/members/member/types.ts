import type { UmbMemberEntityType } from './entity.js';
import type { UmbMemberKindType } from './utils/index.js';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbContentDetailModel, UmbElementValueModel } from '@umbraco-cms/backoffice/content';

export type * from './entity.js';
export type * from './collection/types.js';
export type * from './item/types.js';

export interface UmbMemberDetailModel extends UmbContentDetailModel {
	email: string;
	entityType: UmbMemberEntityType;
	failedPasswordAttempts: number;
	groups: Array<string>;
	isApproved: boolean;
	isLockedOut: boolean;
	isTwoFactorEnabled: boolean;
	kind: UmbMemberKindType;
	lastLockoutDate: string | null;
	lastLoginDate: string | null;
	lastPasswordChangeDate: string | null;
	memberType: {
		unique: string;
		icon: string;
	};
	newPassword?: string;
	oldPassword?: string;
	unique: string;
	username: string;
	values: Array<UmbMemberValueModel>;
	variants: Array<UmbEntityVariantModel>;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberVariantModel extends UmbEntityVariantModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberValueModel<ValueType = unknown> extends UmbElementValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberVariantOptionModel extends UmbEntityVariantOptionModel<UmbMemberVariantModel> {}
