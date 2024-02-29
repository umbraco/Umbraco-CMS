/**
 * Returns a hash code from a string
 * @param  {String} str - The string to hash.
 * @return {Number} - A 32bit integer
 */
export function simpleHashCode(str: string) {
	let hash = 0,
		i = 0;
	const len = str.length;
	while (i < len) {
		hash = (hash << 5) - hash + str.charCodeAt(i++);
		hash |= 0; // Convert to 32bit integer
	}
	return hash;
}
