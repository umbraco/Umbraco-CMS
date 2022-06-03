import { UmbExtensionManifest } from './extension.registry';
import { hasDefaultExport } from './has-default-export.function';
import { isExtensionType } from './is-extension.function';
import { loadExtension } from './load-extension.function';

export async function createExtensionElement(manifest: UmbExtensionManifest): Promise<HTMLElement | undefined> {
  //TODO: Write tests for these extension options:
  const js = await loadExtension(manifest);
  if (manifest.elementName) {
    // created by manifest method providing HTMLElement
    return document.createElement(manifest.elementName);
  }
  if (js) {
    if (js instanceof HTMLElement) {
      console.log('-- created by manifest method providing HTMLElement', js);
      return js;
    }
    if (isExtensionType(js)) {
      // created by js export elementName
      return js.elementName ? document.createElement(js.elementName) : Promise.resolve(undefined);
    }
    if (hasDefaultExport(js)) {
      // created by default class
      return new js.default();
    }
  }
  console.error('-- Extension did not succeed creating an element');
  return Promise.resolve(undefined);
}
