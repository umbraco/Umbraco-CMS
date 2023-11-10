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
}
