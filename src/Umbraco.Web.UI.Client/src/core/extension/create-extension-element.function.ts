import { UmbExtensionManifest } from './extension.registry';
import { loadExtension } from './load-extension.function';

export function createExtensionElement(manifest: UmbExtensionManifest): Promise<HTMLElement> | Promise<undefined> {

  //TODO: Write tests for these extension options:
  return loadExtension(manifest).then((js) => {

    if (manifest.elementName) {
      console.log('-- created by elementName', manifest.elementName);
      return document.createElement(manifest.elementName as any);
    }

    console.log(js)

    if (js) {
      if (js instanceof HTMLElement) {
        console.log('-- created by manifest method providing HTMLElement', js);
        return js;
      }
      if ((js as any).elementName) {
        console.log('-- created by export elementName', (js as any).elementName);
        return document.createElement((js as any).elementName);
      }
      if ((js as any).default) {
        console.log('-- created by default class', (js as any).default);
        return new ((js as any).default) as HTMLElement;
      }
    }

    console.error('-- Extension did not succeed creating an element');
    return Promise.resolve(undefined);
  });
}