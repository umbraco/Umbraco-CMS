import type { ManifestBase, ManifestBundle, ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

/**
 * @deprecated Follow these steps to get Extension Manifest Type for v.15+ projects:
 *
 * Setup your `tsconfig.json` to include the extension-types as global types. Like this:
 * ```
	{
		"compilerOptions": {
			...
			"types": [
				"@umbraco-cms/backoffice/extension-types"
			]
		}
	}
 * ```
 *
 * Once done, you can use the global type `UmbExtensionManifest`.
 *
 * If defining your own extension types, then follow the link below for more information.
 *
 * [Read more on the change announcement]{https://github.com/umbraco/Announcements/issues/22}
 */
export type ManifestTypes = never;

type UmbCoreManifestTypes = ManifestBundle<UmbCoreManifestTypes> | ManifestCondition | ManifestBase;

type UnionOfProperties<T> = T extends object ? T[keyof T] : never;

declare global {
	/**
	 * This global type allows to declare manifests types from its own module.
	 * @example
	 ```js
 	 	declare global {
 	 		interface UmbExtensionManifestMap {
 	 			My_UNIQUE_MANIFEST_NAME: MyExtensionManifestType;
  		}
  	}
  	```
	 If you have multiple types, you can declare them in this way:
	 ```js
		declare global {
			interface UmbExtensionManifestMap {
				My_UNIQUE_MANIFEST_NAME: MyExtensionManifestTypeA | MyExtensionManifestTypeB;
			}
		}
	 ```
	 */
	interface UmbExtensionManifestMap {
		UMB_CORE: UmbCoreManifestTypes;
	}

	/**
	 * This global type provides a union of all declared manifest types.
	 * If this is a local package that declares additional Manifest Types, then these will also be included in this union.
	 */
	type UmbExtensionManifest = UnionOfProperties<UmbExtensionManifestMap>;
}
