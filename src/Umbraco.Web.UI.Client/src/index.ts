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
  manifests.forEach((manifest: UmbExtensionManifest<unknown>) => extensionRegistry.register(manifest));
}

const registerInternalManifests = async () => {
  // TODO: where do we get these from?
  const manifests: Array<UmbExtensionManifest<unknown>> = [
    {
      type: 'section',
      alias: 'Umb.Section.Content',
      name: 'Content',
      elementName: 'umb-content-section',
      meta: {
        weight: 50
      }
    },
    {
      type: 'section',
      alias: 'Umb.Section.Media',
      name: 'Media',
      elementName: 'umb-media-section',
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
      meta: {
        weight: 20
      }
    }
  ];
  manifests.forEach((manifest: UmbExtensionManifest<unknown>) => extensionRegistry.register(manifest));
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