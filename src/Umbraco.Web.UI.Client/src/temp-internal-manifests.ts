import { UmbExtensionManifestCore } from './core/extension';

// TODO: consider moving weight from meta to the main part of the manifest. We need it for every extension.
// TODO: consider adding a label property as part of the meta. It might make sense to have an "extension" name label where one is needed.
export const internalManifests: Array<UmbExtensionManifestCore> = [
	{
		type: 'section',
		alias: 'Umb.Section.Content',
		name: 'Content',
		elementName: 'umb-content-section',
		js: () => import('./backoffice/sections/content/content-section.element'),
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
		js: () => import('./backoffice/sections/media/media-section.element'),
		meta: {
			pathname: 'media', // TODO: how to we want to support pretty urls?
			weight: 50,
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Members',
		name: 'Members',
		elementName: 'umb-members-section',
		meta: {
			pathname: 'members',
			weight: 30,
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Settings',
		name: 'Settings',
		elementName: 'umb-settings-section',
		js: () => import('./backoffice/sections/settings/settings-section.element'),
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
		js: () => import('./backoffice/dashboards/welcome/dashboard-welcome.element'),
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
		js: () => import('./backoffice/dashboards/redirect-management/dashboard-redirect-management.element'),
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
		js: () => import('./backoffice/dashboards/settings-about/dashboard-settings-about.element'),
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
		js: () => import('./backoffice/dashboards/examine-management/dashboard-examine-management.element'),
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
		js: () => import('./backoffice/dashboards/models-builder/dashboard-models-builder.element'),
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
		js: () => import('./backoffice/dashboards/media-management/dashboard-media-management.element'),
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
		js: () => import('./backoffice/property-editors/property-editor-text.element'),
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
		js: () => import('./backoffice/property-editors/property-editor-textarea.element'),
		meta: {
			icon: 'edit',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.ContextExample',
		name: 'Context Example',
		js: () => import('./backoffice/property-editors/property-editor-context-example.element'),
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
		js: () => import('./backoffice/editors/shared/node/views/edit/editor-view-node-edit.element'),
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
		js: () => import('./backoffice/editors/shared/node/views/info/editor-view-node-info.element'),
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
		js: () => import('./backoffice/editors/data-type/views/editor-view-data-type-edit.element'),
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
		js: () => import('./backoffice/editors/document-type/views/editor-view-document-type-design.element'),
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
		js: () => import('./backoffice/property-actions/property-action-copy.element'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUI.Text'],
		},
	},
	{
		type: 'propertyAction',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear',
		elementName: 'umb-property-action-clear',
		js: () => import('./backoffice/property-actions/property-action-clear.element'),
		meta: {
			propertyEditors: ['Umb.PropertyEditorUI.Text'],
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.ContentPicker',
		name: 'ContentPicker',
		elementName: 'umb-property-editor-content-picker',
		js: () => import('./backoffice/property-editors/property-editor-content-picker.element'),
		meta: {
			icon: 'document',
			group: 'common',
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Datatypes',
		name: 'DataTypes',
		elementName: 'umb-datatype-tree',
		js: () => import('./backoffice/tree/datatypes-tree.element'),
		meta: {
			weight: -10,
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DocumentTypes',
		name: 'DocumentTypes',
		elementName: 'umb-document-type-tree',
		js: () => import('./backoffice/tree/document-type-tree.element'),
		meta: {
			weight: -10,
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DocumentTypes',
		name: 'DocumentTypes',
		elementName: 'umb-document-type-tree',
		js: () => import('./backoffice/tree/document-type-tree.element'),
		meta: {
			weight: -10,
			sections: ['Umb.Section.Content'],
		},
	},
];
