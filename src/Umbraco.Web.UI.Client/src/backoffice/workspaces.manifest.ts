// TODO: temp file until we have a way to register from each extension

import type { ManifestWorkspace } from '@umbraco-cms/models';

export const manifests: Array<ManifestWorkspace> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Member',
		name: 'Member Workspace',
		loader: () => import('./members/members/workspace/workspace-member.element'),
		meta: {
			entityType: 'member',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.MemberGroup',
		name: 'Member Group Workspace',
		loader: () => import('./members/member-groups/workspace/workspace-member-group.element'),
		meta: {
			entityType: 'member-group',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.MediaType',
		name: 'Media Type Workspace',
		loader: () => import('./media/media-types/workspace/workspace-media-type.element'),
		meta: {
			entityType: 'media-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.MemberType',
		name: 'Member Type Workspace',
		loader: () => import('./members/member-types/workspace/workspace-member-type.element'),
		meta: {
			entityType: 'member-type',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Extensions',
		name: 'Extensions Workspace',
		loader: () => import('./core/extensions/workspace/extension-root/workspace-extension-root.element'),
		meta: {
			entityType: 'extension-root',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Media',
		name: 'Media Workspace',
		loader: () => import('./media/media/workspace/workspace-media.element'),
		meta: {
			entityType: 'media',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Document',
		name: 'Content Workspace',
		loader: () => import('./documents/documents/workspace/workspace-document.element'),
		meta: {
			entityType: 'document',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.User',
		name: 'User Workspace',
		loader: () => import('../auth/users/workspace/workspace-user.element'),
		meta: {
			entityType: 'user',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.UserGroup',
		name: 'User Group Workspace',
		loader: () => import('../auth/user-groups/workspace/workspace-user-group.element'),
		meta: {
			entityType: 'user-group',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Package',
		name: 'Package Workspace',
		loader: () => import('./packages/package-repo/workspace/workspace-package.element'),
		meta: {
			entityType: 'package',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.PackageBuilder',
		name: 'Package Builder Workspace',
		loader: () => import('./packages/package-builder/workspace/workspace-package-builder.element'),
		meta: {
			entityType: 'package-builder',
		},
	},
	{
		type: 'workspace',
		alias: 'Umb.Workspace.LanguageRoot',
		name: 'Language Root Workspace',
		loader: () => import('./core/languages/workspace/language-root/workspace-language-root.element'),
		meta: {
			entityType: 'language-root',
		},
	},
];
