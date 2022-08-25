import { ManifestTypes } from '../models';

export type ManifestLoaderType = ManifestTypes & { loader: () => Promise<object | HTMLElement> };

export function loadExtension(manifest: ManifestTypes) {
	// TODO: change this back to dynamic import after POC. Vite complains about the dynamic imports in the public folder but doesn't include the temp app_plugins folder in the final build.
	// return import(/* @vite-ignore */ manifest.js);
	if (isManifestLoaderType(manifest)) {
		return manifest.loader();
	}

	return new Promise((resolve, reject) => {
		if (!manifest.js) {
			resolve(null);
			return;
		}

		const script = document.createElement('script');
		script.type = 'text/javascript';
		//script.charset = 'utf-8';
		script.async = true;
		script.type = 'module';
		script.src = manifest.js;
		script.crossOrigin = 'anonymous';
		script.onload = function () {
			resolve(null);
		};
		script.onerror = function () {
			reject(new Error(`Script load error for ${manifest.js}`));
		};
		document.body.appendChild(script);
	}) as Promise<null>;
}

export function isManifestLoaderType(manifest: ManifestTypes): manifest is ManifestLoaderType {
	return typeof (manifest as ManifestLoaderType).loader === 'function';
}
