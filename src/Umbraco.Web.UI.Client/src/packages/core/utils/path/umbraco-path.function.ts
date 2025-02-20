// TODO: Rename to something more obvious, naming wise this can mean anything. I suggest: umbracoManagementApiPath()
/**
 * Generates a path to an Umbraco API endpoint.
 * @param {string} path - The path to the Umbraco API endpoint.
 * @param {string} version - The version of the Umbraco API (default is 'v1').
 * @returns {string} The path to the Umbraco API endpoint.
 */
export function umbracoPath(path: string, version = 'v1') {
	return `/umbraco/management/api/${version}${path}`;
}
