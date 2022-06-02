import { worker } from './mocks/browser';
import { UmbExtensionRegistry, UmbExtensionManifest, UmbExtensionManifestCore } from './core/extension';

const extensionRegistry = new UmbExtensionRegistry();

export interface Umbraco {
  extensionRegistry: UmbExtensionRegistry;
}

declare global {
  interface Window {
    Umbraco: Umbraco;
  }
}

window.Umbraco = {
  extensionRegistry,
};

const registerExtensionManifestsFromServer = async () => {
  // TODO: add schema and use fetcher
  const res = await fetch('/umbraco/backoffice/manifests');
  const { manifests } = await res.json();
  manifests.forEach((manifest: UmbExtensionManifest) => extensionRegistry.register(manifest));
};

const registerInternalManifests = async () => {
  // TODO: where do we get these from?
  const manifests: Array<UmbExtensionManifestCore> = [
    {
      type: 'section',
      alias: 'Umb.Section.Content',
      name: 'Content',
      elementName: 'umb-content-section',
      js: () => import('./extensions/sections/content/content-section.element'),
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
      js: () => import('./extensions/sections/settings/settings-section.element'),
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
      js: () => import('./extensions/dashboards/dashboard-welcome.element'),
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
      js: () => import('./extensions/dashboards/dashboard-redirect-management.element'),
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
      //elementName: 'umb-property-editor-text',
      js: () => import('./extensions/property-editors/property-editor-text.element'),
      meta: {
        icon: 'document',
        group: 'common',
      },
    },
    {
      type: 'propertyEditorUI',
      alias: 'External.PropertyEditorUI.Test',
      name: 'Text',
      //elementName: 'external-property-editor-test', //Gets the element name from JS file.
      js: '/src/property-editors/external-property-editor-test.js',
      meta: {
        icon: 'document',
        group: 'common',
      },
    },
    /*
    {
      type: 'propertyEditorUI',
      alias: 'External.PropertyEditorUI.Test',
      name: 'Text',
      elementName: 'external-property-editor-test', //Gets the element name from JS file.
      js: () => Promise.resolve(document.createElement('hr')),
      meta: {
        icon: 'document',
        group: 'common',
      }
    },
    */
    {
      type: 'propertyEditorUI',
      alias: 'Umb.PropertyEditorUI.Textarea',
      name: 'Textarea',
      elementName: 'umb-property-editor-textarea',
      js: () => import('./extensions/property-editors/property-editor-textarea.element'),
      meta: {
        icon: 'document',
        group: 'common',
      },
    },
  ];

  manifests.forEach((manifest: UmbExtensionManifestCore) =>
    extensionRegistry.register<UmbExtensionManifestCore>(manifest)
  );
};

const setup = async () => {
  await registerExtensionManifestsFromServer();
  await registerInternalManifests();
  // TODO: implement loading of "startUp" extensions
  await import('./app');
};

worker.start({
  onUnhandledRequest: (req) => {
    if (req.url.pathname.startsWith('/node_modules/')) return;
    if (req.url.pathname.startsWith('/src/')) return;
    if (req.destination === 'image') return;

    console.warn('Found an unhandled %s request to %s', req.method, req.url.href);
  },
});

setup();
