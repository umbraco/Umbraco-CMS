import type { ManifestTypes } from './models/index.js';

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
	 * @title An array of Umbraco package manifest types that will be installed
	 * @required
	 */
	extensions: ManifestTypes[];

	/**
	 * @title The importmap for the package
	 * @description This is used to define the imports and the scopes for the package to be used in the browser. It will be combined with the global importmap.
	 * @see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/script/type/importmap
	 */
	importmap?: {
		/**
		 * @title The imports for the package
		 * @required
		 * @minProperties 1
		 * @see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/script/type/importmap#imports
		 */
		imports: Record<string, string>;

		/**
		 * @title The scopes for the package
		 * @see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/script/type/importmap#scopes
		 */
		scopes?: Record<string, object>;
	}
}
