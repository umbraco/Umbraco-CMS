import type { ManifestTypes } from './core/models';

// TODO: consider moving weight from meta to the main part of the manifest. We need it for every extension.
// TODO: consider adding a label property as part of the meta. It might make sense to have an "extension" name label where one is needed.
export const internalManifests: Array<ManifestTypes & { loader: () => Promise<object | HTMLElement> }> = [
	{
		type: 'section',
		alias: 'Umb.Section.Content',
		name: 'Content',
		elementName: 'umb-content-section',
		loader: () => import('./backoffice/sections/content/content-section.element'),
		meta: {
			pathname: 'content', // TODO: how to we want to support pretty urls?
			weight: 50,
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Media',
		name: 'Media',
		elementName: 'umb-media-section',
		loader: () => import('./backoffice/sections/media/media-section.element'),
		meta: {
			pathname: 'media', // TODO: how to we want to support pretty urls?
			weight: 50,
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Members',
		name: 'Members',
		elementName: 'umb-section-members',
		loader: () => import('./backoffice/sections/members/section-members.element'),
		meta: {
			pathname: 'members',
			weight: 30,
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Settings',
		name: 'Settings',
		loader: () => import('./backoffice/sections/settings/settings-section.element'),
		meta: {
			pathname: 'settings', // TODO: how to we want to support pretty urls?
			weight: 20,
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Welcome',
		name: 'Welcome',
		elementName: 'umb-dashboard-welcome',
		loader: () => import('./backoffice/dashboards/welcome/dashboard-welcome.element'),
		meta: {
			sections: ['Umb.Section.Content'],
			pathname: 'welcome', // TODO: how to we want to support pretty urls?
			weight: 20,
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.RedirectManagement',
		name: 'Redirect Management',
		elementName: 'umb-dashboard-redirect-management',
		loader: () => import('./backoffice/dashboards/redirect-management/dashboard-redirect-management.element'),
		meta: {
			sections: ['Umb.Section.Content'],
			pathname: 'redirect-management', // TODO: how to we want to support pretty urls?
			weight: 10,
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.SettingsAbout',
		name: 'Settings About',
		elementName: 'umb-dashboard-settings-about',
		loader: () => import('./backoffice/dashboards/settings-about/dashboard-settings-about.element'),
		meta: {
			label: 'About',
			sections: ['Umb.Section.Settings'],
			pathname: 'about', // TODO: how to we want to support pretty urls?
			weight: 10,
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.ExamineManagement',
		name: 'Examine Management',
		elementName: 'umb-dashboard-examine-management',
		loader: () => import('./backoffice/dashboards/examine-management/dashboard-examine-management.element'),
		meta: {
			sections: ['Umb.Section.Settings'],
			pathname: 'examine-management', // TODO: how to we want to support pretty urls?
			weight: 10,
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.ModelsBuilder',
		name: 'Models Builder',
		elementName: 'umb-dashboard-models-builder',
		loader: () => import('./backoffice/dashboards/models-builder/dashboard-models-builder.element'),
		meta: {
			sections: ['Umb.Section.Settings'],
			pathname: 'models-builder', // TODO: how to we want to support pretty urls?
			weight: 10,
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.MediaManagement',
		name: 'Media',
		elementName: 'umb-dashboard-media-management',
		loader: () => import('./backoffice/dashboards/media-management/dashboard-media-management.element'),
		meta: {
			sections: ['Umb.Section.Media'],
			pathname: 'media-management', // TODO: how to we want to support pretty urls?
			weight: 10,
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.Text',
		name: 'Text',
		loader: () => import('./backoffice/property-editors/property-editor-text.element'),
		meta: {
			icon: 'edit',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.Textarea',
		name: 'Textarea',
		elementName: 'umb-property-editor-textarea',
		loader: () => import('./backoffice/property-editors/property-editor-textarea.element'),
		meta: {
			icon: 'edit',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.ContextExample',
		name: 'Context Example',
		loader: () => import('./backoffice/property-editors/property-editor-context-example.element'),
		meta: {
			icon: 'favorite',
			group: 'common',
		},
	},
	{
		type: 'editorView',
		alias: 'Umb.EditorView.ContentEdit',
		name: 'Content',
		elementName: 'umb-editor-view-node-edit',
		loader: () => import('./backoffice/editors/shared/node/views/edit/editor-view-node-edit.element'),
		meta: {
			// TODO: how do we want to filter where editor views are shown? https://our.umbraco.com/documentation/extending/Content-Apps/#setting-up-the-plugin
			// this is a temp solution
			editors: ['Umb.Editor.Content', 'Umb.Editor.Media'],
			pathname: 'content',
			weight: 100,
			icon: 'document',
		},
	},
	{
		type: 'editorView',
		alias: 'Umb.EditorView.ContentInfo',
		name: 'Info',
		elementName: 'umb-editor-view-node-info',
		loader: () => import('./backoffice/editors/shared/node/views/info/editor-view-node-info.element'),
		meta: {
			// TODO: how do we want to filter where editor views are shown? https://our.umbraco.com/documentation/extending/Content-Apps/#setting-up-the-plugin
			// this is a temp solution
			editors: ['Umb.Editor.Content', 'Umb.Editor.Media'],
			pathname: 'info',
			weight: 90,
			icon: 'info',
		},
	},
	{
		type: 'editorView',
		alias: 'Umb.EditorView.DataTypeEdit',
		name: 'Edit',
		elementName: 'umb-editor-view-data-type-edit',
		loader: () => import('./backoffice/editors/data-type/views/editor-view-data-type-edit.element'),
		meta: {
			// TODO: how do we want to filter where editor views are shown? https://our.umbraco.com/documentation/extending/Content-Apps/#setting-up-the-plugin
			// this is a temp solution
			editors: ['Umb.Editor.DataType'],
			pathname: 'edit',
			weight: 90,
			icon: 'edit',
		},
	},
	{
		type: 'editorView',
		alias: 'Umb.EditorView.DocumentTypeDesign',
		name: 'Design',
		elementName: 'umb-editor-view-document-type-design',
		loader: () => import('./backoffice/editors/document-type/views/editor-view-document-type-design.element'),
		meta: {
			// TODO: how do we want to filter where editor views are shown? https://our.umbraco.com/documentation/extending/Content-Apps/#setting-up-the-plugin
			// this is a temp solution
			editors: ['Umb.Editor.DocumentType'],
			pathname: 'design',
			weight: 90,
			icon: 'edit',
		},
	},
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Copy',
		name: 'Copy',
		elementName: 'umb-property-action-copy',
		loader: () => import('./backoffice/property-actions/property-action-copy.element'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUI.Text'],
		},
	},
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear',
		elementName: 'umb-property-action-clear',
		loader: () => import('./backoffice/property-actions/property-action-clear.element'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUI.Text'],
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.ContentPicker',
		name: 'ContentPicker',
		elementName: 'umb-property-editor-content-picker',
		loader: () => import('./backoffice/property-editors/property-editor-content-picker.element'),
		meta: {
			icon: 'document',
			group: 'common',
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Datatypes',
		name: 'Data Types Tree',
		loader: () => import('./backoffice/tree/data-types/tree-data-types.element'),
		meta: {
			pathname: 'data-types',
			editor: 'Umb.Editor.DataType',
			label: 'Data Types',
			weight: 1,
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DocumentTypes',
		name: 'Document Types Tree',
		elementName: 'umb-document-type-tree',
		loader: () => import('./backoffice/tree/document-types/tree-document-types.element'),
		meta: {
			pathname: 'document-types',
			editor: 'Umb.Editor.DocumentType',
			label: 'Document Types',
			weight: 2,
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.MembersTest',
		name: 'Members Test',
		elementName: 'umb-dashboard-welcome',
		loader: () => import('./backoffice/dashboards/welcome/dashboard-welcome.element'),
		meta: {
			weight: -10,
			pathname: 'welcome',
			sections: ['Umb.Section.Members'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Members',
		name: 'Members Tree',
		loader: () => import('./backoffice/tree/members/tree-members.element'),
		meta: {
			editor: 'Umb.Editor.Member',
			pathname: 'members',
			label: 'Members',
			weight: 0,
			sections: ['Umb.Section.Members'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.MemberGroups',
		name: 'Members Groups Tree',
		loader: () => import('./backoffice/tree/member-groups/tree-member-groups.element'),
		meta: {
			editor: 'Umb.Editor.MemberGroup',
			pathname: 'member-groups',
			label: 'Member Groups',
			weight: 1,
			sections: ['Umb.Section.Members'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Extensions',
		name: 'Extensions Tree',
		loader: () => import('./backoffice/tree/extensions/tree-extensions.element'),
		meta: {
			editor: 'Umb.Editor.Extensions',
			pathname: 'extensions',
			label: 'Extensions',
			weight: 3,
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Member',
		name: 'Member Editor',
		loader: () => import('./backoffice/editors/member/editor-member.element'),
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.MemberGroup',
		name: 'Member Group Editor',
		loader: () => import('./backoffice/editors/member-group/editor-member-group.element'),
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.DataType',
		name: 'Data Type Editor',
		loader: () => import('./backoffice/editors/data-type/editor-data-type.element'),
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.DocumentType',
		name: 'Document Type Editor',
		loader: () => import('./backoffice/editors/document-type/editor-document-type.element'),
	},
	{
		type: 'editor',
		alias: 'Umb.Editor.Extensions',
		name: 'Extensions Editor',
		loader: () => import('./backoffice/editors/extensions/editor-extensions.element'),
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Create',
		name: 'Document Type Create',
		loader: () => import('./backoffice/editors/extensions/editor-extensions.element'),
		meta: {
			label: 'Create',
			icon: 'plus',
			weight: 100,
		},
	},
];
