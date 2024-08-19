import type { UmbMemberEntityType } from './entity.js';
import type { UmbVariantModel, UmbVariantOptionModel } from '@umbraco-cms/backoffice/variant';

export interface UmbMemberDetailModel {
	email: string;
	entityType: UmbMemberEntityType;
	failedPasswordAttempts: number;
	groups: Array<string>;
	isApproved: boolean;
	isLockedOut: boolean;
	isTwoFactorEnabled: boolean;
	lastLockoutDate: string | null;
	lastLoginDate: string | null;
	lastPasswordChangeDate: string | null;
	memberType: { unique: string };
	newPassword?: string;
	oldPassword?: string;
	unique: string;
	username: string;
	values: Array<UmbMemberValueModel>;
	variants: Array<UmbVariantModel>;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberVariantModel extends UmbVariantModel {}

export interface UmbMemberValueModel<ValueType = unknown> {
	culture: string | null;
	segment: string | null;
	alias: string;
	value: ValueType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberVariantOptionModel extends UmbVariantOptionModel<UmbMemberVariantModel> {}
