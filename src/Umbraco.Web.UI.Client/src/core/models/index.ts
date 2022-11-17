import type { UserStatus } from '../../backoffice/sections/users/user-extensions';
import {
	ContentTreeItem,
	DocumentTreeItem,
	DocumentTypeTreeItem,
	EntityTreeItem,
	FolderTreeItem,
} from '@umbraco-cms/backend-api';

// Extension Manifests
export * from '../extensions-registry/models';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type HTMLElementConstructor<T = HTMLElement> = new (...args: any[]) => T;

// Users
export interface Entity {
	key: string;
	name: string;
	icon: string;
	type: string;
	hasChildren: boolean;
	parentKey: string;
	isTrashed: boolean;
}

export interface UserEntity extends Entity {
	type: 'user';
}

export interface UserDetails extends UserEntity {
	email: string;
	status: UserStatus;
	language: string;
	lastLoginDate?: string;
	lastLockoutDate?: string;
	lastPasswordChangeDate?: string;
	updateDate: string;
	createDate: string;
	failedLoginAttempts: number;
	userGroup?: string; //TODO Implement this
}

export interface UserGroupEntity extends Entity {
	type: 'userGroup';
}

export interface UserGroupDetails extends UserGroupEntity {
	key: string;
	name: string;
	icon: string;
	sections?: Array<string>;
	contentStartNode?: string;
	mediaStartNode?: string;
}

// Data Types
export interface DataTypeDetails extends FolderTreeItem {
	key: string; // TODO: Remove this when the backend is fixed
	isTrashed: boolean; // TODO: remove only temp part of refactor
	propertyEditorModelAlias: string | null;
	propertyEditorUIAlias: string | null;
	data: Array<DataTypePropertyData>;
}

export interface DataTypePropertyData {
	alias: string;
	value: any;
}

// Document Types
export interface DocumentTypeDetails extends DocumentTypeTreeItem {
	key: string; // TODO: Remove this when the backend is fixed
	isTrashed: boolean; // TODO: remove only temp part of refactor
	alias: string;
	properties: [];
}

export interface MemberTypeDetails extends EntityTreeItem {
	key: string; // TODO: Remove this when the backend is fixed
	alias: string;
	properties: [];
}

// Content
export interface ContentProperty {
	alias: string;
	label: string;
	description: string;
	dataTypeKey: string;
}

export interface ContentPropertyData {
	alias: string;
	value: any;
}

// Documents
export interface DocumentDetails extends DocumentTreeItem {
	key: string; // TODO: Remove this when the backend is fixed
	isTrashed: boolean; // TODO: remove only temp part of refactor
	properties: Array<ContentProperty>;
	data: Array<ContentPropertyData>;
	variants: Array<any>; // TODO: define variant data
	//layout?: any; // TODO: define layout type - make it non-optional
}

// Media
export interface MediaDetails extends ContentTreeItem {
	key: string; // TODO: Remove this when the backend is fixed
	properties: Array<ContentProperty>;
	data: Array<ContentPropertyData>;
	variants: Array<any>; // TODO: define variant data
	//layout?: any; // TODO: define layout type - make it non-optional
}

// Media Types

export interface MediaTypeDetails extends EntityTreeItem {
	key: string; // TODO: Remove this when the backend is fixed
	alias: string;
	properties: [];
}

// Member Groups
export interface MemberGroupDetails extends EntityTreeItem {
	key: string; // TODO: Remove this when the backend is fixed
}
