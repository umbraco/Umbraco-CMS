import type { ManifestWorkspace } from '@umbraco-cms/models';

export const manifests: Array<ManifestWorkspace> = [
	{
		type: 'workspace',
		alias: 'Umb.Editor.Member',
		name: 'Member Editor',
		loader: () => import('./member/workspace-member.element'),
		meta: {
			entityType: 'member',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Editor.MemberGroup',
		name: 'Member Group Editor',
		loader: () => import('./member-group/workspace-member-group.element'),
		meta: {
			entityType: 'member-group',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Editor.DataType',
		name: 'Data Type Editor',
		loader: () => import('./data-type/editor-data-type.element'),
		meta: {
			entityType: 'data-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.DocumentType',
		name: 'Document Type Editor',
		loader: () => import('./document-type/workspace-document-type.element'),
		meta: {
			entityType: 'document-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Editor.MediaType',
		name: 'Media Type Editor',
		loader: () => import('./media-type/workspace-media-type.element'),
		meta: {
			entityType: 'media-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Editor.MemberType',
		name: 'Member Type Editor',
		loader: () => import('./member-type/workspace-member-type.element'),
		meta: {
			entityType: 'member-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Editor.Extensions',
		name: 'Extensions Editor',
		loader: () => import('./extensions/workspace-extensions.element'),
		meta: {
			entityType: 'extensions-list',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Editor.Media',
		name: 'Media Editor',
		loader: () => import('./media/workspace-media.element'),
		meta: {
			entityType: 'media',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Document',
		name: 'Content Editor',
		loader: () => import('./document/editor-document.element'),
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
		alias: 'Umb.Editor.Package',
		name: 'Package Editor',
		loader: () => import('./package/workspace-package.element'),
		meta: {
			entityType: 'package',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Editor.PackageBuilder',
		name: 'Package Builder Editor',
		loader: () => import('./package-builder/workspace-package-builder.element'),
		meta: {
			entityType: 'package-builder',
		},
	},
];
