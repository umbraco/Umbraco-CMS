import type { ManifestTypes, ManifestWithLoader } from '@umbraco-cms/models';

export const internalManifests: Array<ManifestWithLoader<ManifestTypes>> = [
	{
		type: 'editorView',
		alias: 'Umb.EditorView.DocumentType.Design',
		name: 'Document Type Editor Design View',
		elementName: 'umb-editor-view-document-type-design',
		loader: () => import('../backoffice/editors/document-type/views/editor-view-document-type-design.element'),
		weight: 90,
		meta: {
			editors: ['Umb.Editor.DocumentType'],
			label: 'Design',
			pathname: 'design',
			icon: 'edit',
		},
	},
	{
		type: 'editorView',
		alias: 'Umb.Editor.Packages.Overview',
		name: 'Packages Editor Overview View',
		elementName: 'umb-packages-overview',
		loader: () => import('../backoffice/sections/packages/packages-overview.element'),
		weight: 10,
		meta: {
			icon: 'document',
			label: 'Packages',
			pathname: 'repo',
			editors: ['Umb.Editor.Packages'],
		},
	},
	{
		type: 'editorView',
		alias: 'Umb.Editor.Packages.Installed',
		name: 'Packages Editor Installed View',
		elementName: 'umb-packages-installed',
		loader: () => import('../backoffice/sections/packages/packages-installed.element'),
		weight: 0,
		meta: {
			icon: 'document',
			label: 'Installed',
			pathname: 'installed',
			editors: ['Umb.Editor.Packages'],
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.User',
		name: 'User Editor',
		loader: () => import('../backoffice/editors/user/editor-user.element'),
		meta: {
			entityType: 'user',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.UserGroup',
		name: 'User Group Editor',
		loader: () => import('../backoffice/editors/user-group/editor-user-group.element'),
		meta: {
			entityType: 'userGroup',
		},
	},
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Copy',
		name: 'Copy Property Action',
		elementName: 'umb-property-action-copy',
		loader: () => import('../backoffice/property-actions/copy/property-action-copy.element'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUI.TextBox'],
		},
	},
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear Property Action',
		elementName: 'umb-property-action-clear',
		loader: () => import('../backoffice/property-actions/clear/property-action-clear.element'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUI.TextBox'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DataTypes',
		name: 'Data Types Tree',
		loader: () => import('../backoffice/trees/data-types/tree-data-types.element'),
		weight: 1,
		meta: {
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DocumentTypes',
		name: 'Document Types Tree',
		loader: () => import('../backoffice/trees/document-types/tree-document-types.element'),
		weight: 2,
		meta: {
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Members',
		name: 'Members Tree',
		loader: () => import('../backoffice/trees/members/tree-members.element'),
		weight: 0,
		meta: {
			sections: ['Umb.Section.Members'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.MemberGroups',
		name: 'Members Groups Tree',
		loader: () => import('../backoffice/trees/member-groups/tree-member-groups.element'),
		weight: 1,
		meta: {
			sections: ['Umb.Section.Members'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Extensions',
		name: 'Extensions Tree',
		loader: () => import('../backoffice/trees/extensions/tree-extensions.element'),
		weight: 3,
		meta: {
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Media',
		name: 'Media Tree',
		loader: () => import('../backoffice/trees/media/tree-media.element'),
		weight: 100,
		meta: {
			sections: ['Umb.Section.Media'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Documents',
		name: 'Documents Tree',
		loader: () => import('../backoffice/trees/documents/tree-documents.element'),
		weight: 100,
		meta: {
			sections: ['Umb.Section.Content'],
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Member',
		name: 'Member Editor',
		loader: () => import('../backoffice/editors/member/editor-member.element'),
		meta: {
			entityType: 'member',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.MemberGroup',
		name: 'Member Group Editor',
		loader: () => import('../backoffice/editors/member-group/editor-member-group.element'),
		meta: {
			entityType: 'memberGroup',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.DataType',
		name: 'Data Type Editor',
		loader: () => import('../backoffice/editors/data-type/editor-data-type.element'),
		meta: {
			entityType: 'dataType',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.DocumentType',
		name: 'Document Type Editor',
		loader: () => import('../backoffice/editors/document-type/editor-document-type.element'),
		meta: {
			entityType: 'documentType',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Extensions',
		name: 'Extensions Editor',
		loader: () => import('../backoffice/editors/extensions/editor-extensions.element'),
		meta: {
			entityType: 'extensionsList',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Media',
		name: 'Media Editor',
		loader: () => import('../backoffice/editors/media/editor-media.element'),
		meta: {
			entityType: 'media',
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Document',
		name: 'Content Editor',
		loader: () => import('../backoffice/editors/document/editor-document.element'),
		meta: {
			entityType: 'document',
		},
	},
];
