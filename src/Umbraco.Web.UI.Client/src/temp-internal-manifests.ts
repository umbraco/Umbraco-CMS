import { UmbExtensionManifestCore } from './core/extension';

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
    js: () => import('./backoffice/dashboards/dashboard-welcome.element'),
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
    js: () => import('./backoffice/dashboards/dashboard-redirect-management.element'),
    meta: {
      sections: ['Umb.Section.Content'],
      pathname: 'redirect-management', // TODO: how to we want to support pretty urls?
      weight: 10,
    },
  },
  {
    type: 'propertyEditorUI',
    alias: 'Umb.PropertyEditorUI.Text',
    name: 'Text',
    js: () => import('./backoffice/property-editors/property-editor-text.element'),
    meta: {
      icon: 'document',
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
      icon: 'document',
      group: 'common',
    },
  },
  {
    type: 'propertyEditorUI',
    alias: 'Umb.PropertyEditorUI.ContextExample',
    name: 'Context Example',
    js: () => import('./backoffice/property-editors/property-editor-context-example.element'),
    meta: {
      icon: 'document',
      group: 'common',
    },
  },
  {
    type: 'editorView',
    alias: 'Umb.EditorView.ContentEdit',
    name: 'Content',
    elementName: 'umb-editor-view-node-edit',
    js: () => import('./backoffice/editor-views/editor-view-node-edit.element'),
    meta: {
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
    js: () => import('./backoffice/editor-views/editor-view-node-info.element'),
    meta: {
      pathname: 'info',
      weight: 90,
      icon: 'info',
    }
  }
];