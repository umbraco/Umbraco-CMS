import { hasDefaultExport } from './has-default-export.function.js';
import { isManifestClassConstructorType } from './type-guards/index.js';
import { loadExtension } from './load-extension.function.js';
import type { ManifestApi, ClassConstructor } from './types.js';

//TODO: Write tests for this method:
export async function createExtensionApi<ApiType = unknown>(
	manifest: ManifestApi,
	constructorArguments: unknown[]
): Promise<ApiType | undefined> {
	const js = await loadExtension(manifest);

	if (isManifestClassConstructorType<ApiType>(manifest)) {
		return new manifest.api(...constructorArguments);
	}

	if (js) {
		if (hasDefaultExport<ClassConstructor<ApiType>>(js)) {
			return new js.default(...constructorArguments);
		}

		console.error(
			'-- Extension did not succeed creating an api class instance, missing a default export of the served JavaScript file',
			manifest
		);

		return undefined;
	}

	console.error(
		'-- Extension did not succeed creating an api class instance, missing a default export or `api` in the manifest.',
		manifest
	);
	return undefined;
}
