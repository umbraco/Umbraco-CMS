/**
 * Validation Data Path filter for Block Content Data.
 * write a JSON-Path filter similar to `?(@.udi = 'my-udi://1234')`
 * @param udi {string} - The udi of the block content data.
 * @returns
 */
export function UmbDataPathBlockContentDataFilter(udi: string): string {
	// write a array of strings for each property, where alias must be present and culture and segment are optional
	//const filters: Array<string> = [`@.udi = '${udi}'`];
	//return `?(${filters.join(' && ')})`;
	return `?(@.udi = '${udi}')`;
}
