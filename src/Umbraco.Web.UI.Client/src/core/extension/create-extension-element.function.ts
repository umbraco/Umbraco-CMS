import { UmbExtensionManifest } from './extension.registry';
import { hasDefaultExport } from './has-default-export.function';
import { isExtensionType } from './is-extension.function';
import { loadExtension } from './load-extension.function';

export async function createExtensionElement(manifest: UmbExtensionManifest): Promise<HTMLElement | undefined> {
  //TODO: Write tests for these extension options:
  const js = await loadExtension(manifest);
  if (manifest.elementName) {
    console.log('-- created by elementName', manifest.elementName);
    return document.createElement(manifest.elementName);
  }
  if (js) {
    if (js instanceof HTMLElement) {
      console.log('-- created by manifest method providing HTMLElement', js);
      return js;
    }
    if (isExtensionType(js)) {
      console.log('-- created by export elementName', js.elementName);
      return js.elementName ? document.createElement(js.elementName) : Promise.resolve(undefined);
    }
    if (hasDefaultExport(js)) {
      console.log('-- created by default class', js.default);
      return new js.default();
    }
  }
  console.error('-- Extension did not succeed creating an element');
  return Promise.resolve(undefined);
}
