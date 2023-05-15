import { hasDefaultExport } from './has-default-export.function';
import { isManifestClassConstructorType } from './type-guards';
import { loadExtension } from './load-extension.function';
import type { ManifestClass, ClassConstructor } from './types';

//TODO: Write tests for this method:
export async function createExtensionClass<T = unknown>(
	manifest: ManifestClass,
	constructorArguments: unknown[]
): Promise<T | undefined> {
	const js = await loadExtension(manifest);

	if (isManifestClassConstructorType(manifest)) {
		return new manifest.class(...constructorArguments) as T;
	}

	if (js) {
		if (hasDefaultExport<ClassConstructor<T>>(js)) {
			return new js.default(...constructorArguments);
		}

		console.error(
			'-- Extension did not succeed creating an class instance, missing a default export of the served JavaScript file',
			manifest
		);

		return undefined;
	}

	console.error(
		'-- Extension did not succeed creating an class instance, missing a default export or `class` in the manifest.',
		manifest
	);
	return undefined;
}
