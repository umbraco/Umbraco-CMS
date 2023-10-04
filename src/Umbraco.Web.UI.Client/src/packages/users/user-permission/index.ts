export * from './components/index.js';
export * from './conditions/index.js';

export type UserPermissionModel<PermissionTargetType> = {
	id: string;
	target: PermissionTargetType;
	permissions: Array<string>;
};

// TODO: this should only be known by the document
export type UmbDocumentGranularPermission = {
	entityType: 'document';
	documentId: string;
	userGroupId: string;
};
