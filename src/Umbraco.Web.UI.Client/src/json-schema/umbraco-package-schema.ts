import '@umbraco-cms/backoffice/extension-registry';

/**
 * Umbraco package manifest JSON
 */
export interface UmbracoPackage {
	/**
	 * @title The unique identifier of the Umbraco package
	 */
	id?: string;

	/**
	 * @title The name of the Umbraco package
	 * @required
	 */
	name: string;

	/**
	 * @title The version of the Umbraco package in the style of semver
	 * @examples ["0.1.0"]
	 */
	version?: string;

	/**
	 * @title Decides if the package sends telemetry data for collection
	 * @default true
	 */
	allowTelemetry?: boolean;

	/**
	 * @title Decides if the package is allowed to be accessed by the public, e.g. on the login screen
	 * @default false
	 */
	allowPublicAccess?: boolean;

	/**
	 * @title An array of Umbraco package manifest types that will be installed
	 * @required
	 */
	extensions: UmbExtensionManifest[];

	/**
	 * @title The importmap for the package
	 * @description This is used to define the imports and the scopes for the package to be used in the browser. It will be combined with the global importmap.
	 * @see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/script/type/importmap
	 */
	importmap?: UmbracoPackageImportmap;
}

export interface UmbracoPackageImportmap {
	/**
	 * @title A module specifier with a path for the importmap
	 * @description This is used to define the module specifiers and their respective paths for the package to be used in the browser.
	 * @examples [{
	 * 		"library": "./path/to/library.js",
	 * 		"library/*": "./path/to/library/*"
	 * }]
	 * @required
	 * @minProperties 1
	 * @see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/script/type/importmap#imports
	 */
	imports: UmbracoPackageImportmapValue;

	/**
	 * @title The importmap scopes for the package
	 * @description This is used to define the scopes for the package to be used in the browser. It has to specify a path and a value that is an object with module specifiers and their respective paths.
	 * @examples [{
	 * 		"/path/to/library": { "library": "./path/to/some/other/library.js" }
	 * }]
	 * @see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/script/type/importmap#scopes
	 */
	scopes?: UmbracoPackageImportmapScopes;
}

export interface UmbracoPackageImportmapScopes {
	[key: string]: UmbracoPackageImportmapValue;
}

export interface UmbracoPackageImportmapValue {
	[key: string]: string;
}
