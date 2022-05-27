import { worker } from './mocks/browser';
import { UmbExtensionRegistry, UmbExtensionManifest } from './core/extension';

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
  extensionRegistry
};

const registerExtensionManifestsFromServer = async () => {
  // TODO: add schema and use fetcher
  const res = await fetch('/umbraco/backoffice/manifests');
  const { manifests } = await res.json();
  manifests.forEach((manifest: UmbExtensionManifest) => extensionRegistry.register(manifest));
}

const registerInternalManifests = async () => {
  // TODO: where do we get these from?
  const manifests: Array<UmbExtensionManifest> = [
    {
      type: 'section',
      alias: 'Umb.Section.Content',
      name: 'Content',
      elementName: 'umb-content-section',
      js: () => import('./content/content-section.element'),
      meta: {
        weight: 50
      }
    },
    {
      type: 'section',
      alias: 'Umb.Section.Media',
      name: 'Media',
      elementName: 'umb-media-section',
      js: () => import('./media/media-section.element'),
      meta: {
        weight: 40
      }
    },
    {
      type: 'section',
      alias: 'Umb.Section.Members',
      name: 'Members',
      elementName: 'umb-members-section',
      meta: {
        weight: 30
      }
    },
    {
      type: 'section',
      alias: 'Umb.Section.Settings',
      name: 'Settings',
      elementName: 'umb-settings-section',
      js: () => import('./settings/settings-section.element'),
      meta: {
        weight: 20
      }
    },
    {
      type: 'dashboard',
      alias: 'Umb.Dashboard.Welcome',
      name: 'Welcome',
      elementName: 'umb-dashboard-welcome',
      js: () => import('./dashboards/dashboard-welcome.element'),
      meta: {
        sections: ['Umb.Section.Content'],
        weight: 20
      }
    },
    {
      type: 'dashboard',
      alias: 'Umb.Dashboard.RedirectManagement',
      name: 'Redirect Management',
      elementName: 'umb-dashboard-redirect-management',
      js: () => import('./dashboards/dashboard-redirect-management.element'),
      meta: {
        sections: ['Umb.Section.Content'],
        weight: 10
      }
    }
  ];
  manifests.forEach((manifest: UmbExtensionManifest) => extensionRegistry.register(manifest));

  extensionRegistry.register({
    type: 'propertyEditor',
    alias: 'Umb.PropertyEditor.MyPropertyEditor',
    name: 'Settings',
    elementName: 'umb-settings-section',
    meta: {
      test: 20
    }
  });
}

const setup = async () => {
  await registerExtensionManifestsFromServer();
  await registerInternalManifests();
  // TODO: implement loading of "startUp" extensions
  await import('./app');
}

worker.start({
  onUnhandledRequest: (req) => {
    if (req.url.pathname.startsWith('/node_modules/')) return;
    if (req.url.pathname.startsWith('/src/')) return;
    if (req.destination === 'image') return;

    console.warn(
      'Found an unhandled %s request to %s',
      req.method,
      req.url.href,
    )
  }
});

setup();