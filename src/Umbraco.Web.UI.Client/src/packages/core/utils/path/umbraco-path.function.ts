// TODO: Rename to something more obvious, naming wise this can mean anything. I suggest: umbracoManagementApiPath()
/**
 *
 * @param path
 */
export function umbracoPath(path: string, version = 'v1') {
	return `/umbraco/management/api/${version}${path}`;
}
