import type { ManifestEditor } from '@umbraco-cms/models';

export const manifests: Array<ManifestEditor> = [
	{
		type: 'editor',
		alias: 'Umb.Editor.Member',
		name: 'Member Editor',
		loader: () => import('./member/editor-member.element'),
		meta: {
			entityType: 'member',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.MemberGroup',
		name: 'Member Group Editor',
		loader: () => import('./member-group/editor-member-group.element'),
		meta: {
			entityType: 'memberGroup',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.DataType',
		name: 'Data Type Editor',
		loader: () => import('./data-type/editor-data-type.element'),
		meta: {
			entityType: 'dataType',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.DocumentType',
		name: 'Document Type Editor',
		loader: () => import('./document-type/editor-document-type.element'),
		meta: {
			entityType: 'documentType',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Extensions',
		name: 'Extensions Editor',
		loader: () => import('./extensions/editor-extensions.element'),
		meta: {
			entityType: 'extensionsList',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Media',
		name: 'Media Editor',
		loader: () => import('./media/editor-media.element'),
		meta: {
			entityType: 'media',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Document',
		name: 'Content Editor',
		loader: () => import('./document/editor-document.element'),
		meta: {
			entityType: 'document',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.User',
		name: 'User Editor',
		loader: () => import('./user/editor-user.element'),
		meta: {
			entityType: 'user',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.UserGroup',
		name: 'User Group Editor',
		loader: () => import('./user-group/editor-user-group.element'),
		meta: {
			entityType: 'userGroup',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Package',
		name: 'Package Editor',
		loader: () => import('./package/editor-package.element'),
		meta: {
			entityType: 'package',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.PackageBuilder',
		name: 'Package Builder Editor',
		loader: () => import('./package-builder/editor-package-builder.element'),
		meta: {
			entityType: 'packageBuilder',
		},
	},
];
