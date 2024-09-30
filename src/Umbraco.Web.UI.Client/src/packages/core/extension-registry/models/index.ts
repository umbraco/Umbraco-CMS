import type { ManifestAuthProvider } from './auth-provider.model.js';
import type { ManifestDynamicRootOrigin, ManifestDynamicRootQueryStep } from './dynamic-root.model.js';
import type { ManifestFileUploadPreview } from './file-upload-preview.model.js';
import type { ManifestExternalLoginProvider } from './external-login-provider.model.js';
import type { ManifestTinyMcePlugin } from './tinymce-plugin.model.js';
import type { ManifestUfmComponent } from './ufm-component.model.js';
import type { ManifestUfmFilter } from './ufm-filter.model.js';
import type { ManifestMfaLoginProvider } from './mfa-login-provider.model.js';
import type { ManifestBase, ManifestBundle, ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export type * from './auth-provider.model.js';
export type * from './dynamic-root.model.js';
export type * from './file-upload-preview.model.js';
export type * from './external-login-provider.model.js';
export type * from './mfa-login-provider.model.js';
export type * from './tinymce-plugin.model.js';
export type * from './ufm-component.model.js';
export type * from './ufm-filter.model.js';

export type ManifestTypes =
	| ManifestAuthProvider
	| ManifestBundle<ManifestTypes>
	| ManifestCondition
	| ManifestDynamicRootOrigin
	| ManifestDynamicRootQueryStep
	| ManifestFileUploadPreview
	| ManifestExternalLoginProvider
	| ManifestMfaLoginProvider
	| ManifestTinyMcePlugin
	| ManifestUfmComponent
	| ManifestUfmFilter
	| ManifestBase;

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
		UMB_CORE: ManifestTypes;
	}

	/**
	 * This global type provides a union of all declared manifest types.
	 * If this is a local package that declares additional Manifest Types, then these will also be included in this union.
	 */
	type UmbExtensionManifest = UnionOfProperties<UmbExtensionManifestMap>;
}
