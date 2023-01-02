import type { ManifestWorkspace } from '@umbraco-cms/models';

export const manifests: Array<ManifestWorkspace> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Member',
		name: 'Member Workspace',
		loader: () => import('./member/workspace-member.element'),
		meta: {
			entityType: 'member',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.MemberGroup',
		name: 'Member Group Workspace',
		loader: () => import('./member-group/workspace-member-group.element'),
		meta: {
			entityType: 'member-group',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.DataType',
		name: 'Data Type Workspace',
		loader: () => import('./data-type/workspace-data-type.element'),
		meta: {
			entityType: 'data-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.DocumentType',
		name: 'Document Type Workspace',
		loader: () => import('./document-type/workspace-document-type.element'),
		meta: {
			entityType: 'document-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.MediaType',
		name: 'Media Type Workspace',
		loader: () => import('./media-type/workspace-media-type.element'),
		meta: {
			entityType: 'media-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.MemberType',
		name: 'Member Type Workspace',
		loader: () => import('./member-type/workspace-member-type.element'),
		meta: {
			entityType: 'member-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Extensions',
		name: 'Extensions Workspace',
		loader: () => import('./extension-root/workspace-extension-root.element'),
		meta: {
			entityType: 'extension-root',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Media',
		name: 'Media Workspace',
		loader: () => import('./media/workspace-media.element'),
		meta: {
			entityType: 'media',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Document',
		name: 'Content Workspace',
		loader: () => import('./document/workspace-document.element'),
		meta: {
			entityType: 'document',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.User',
		name: 'User Workspace',
		loader: () => import('./user/workspace-user.element'),
		meta: {
			entityType: 'user',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.UserGroup',
		name: 'User Group Workspace',
		loader: () => import('./user-group/workspace-user-group.element'),
		meta: {
			entityType: 'user-group',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Package',
		name: 'Package Workspace',
		loader: () => import('./package/workspace-package.element'),
		meta: {
			entityType: 'package',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.PackageBuilder',
		name: 'Package Builder Workspace',
		loader: () => import('./package-builder/workspace-package-builder.element'),
		meta: {
			entityType: 'package-builder',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.LanguageRoot',
		name: 'Language Root Workspace',
		loader: () => import('./language-root/workspace-language-root.element'),
		meta: {
			entityType: 'language-root',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Dictionary',
		name: 'Dictionary Workspace',
		loader: () => import('./dictionary/workspace-dictionary.element'),
		meta: {
			entityType: 'dictionary',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.DocumentBlueprintRoot',
		name: 'Document Blueprint Root Workspace',
		loader: () => import('./document-blueprint/workspace-document-blueprint-root.element'),
		meta: {
			entityType: 'document-blueprint-root',
		},
	},
];
