import { UmbExtensionManifest } from './extension.registry';

export function loadExtension(manifest: UmbExtensionManifest): Promise<object | HTMLElement> | Promise<null> {
  if (typeof manifest.js === 'function') {
    return manifest.js() as Promise<object | HTMLElement>;
  }

  // TODO: verify if this is acceptable solution.
  if (typeof manifest.js === 'string') {
    // TODO: change this back to dynamic import after POC. Vite complains about the dynamic imports in the public folder but doesn't include the temp app_plugins folder in the final build.
    // return import(/* @vite-ignore */ manifest.js);
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.type = 'text/javascript';
      //script.charset = 'utf-8';
      script.async = true;
      script.type = 'module';
      script.src = manifest.js as string;
      script.onload = function () {
        resolve(null);
      };
      script.onerror = function () {
        reject(new Error(`Script load error for ${manifest.js}`));
      };
      document.body.appendChild(script);
    }) as Promise<null>;
  }

  console.log('-- Extension does not have any referenced JS');
  return Promise.resolve(null);
}
