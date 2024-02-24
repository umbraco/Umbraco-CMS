import { toCamelCase } from './to-camel-case/to-camel-case.function.js';

export function generateAlias(text: string) {
	//replace all spaces characters with a dash and remove all non-alphanumeric characters, except underscore. Allow a maximum of 1 dashes or underscores in a row.
	//Do we really want that and not the original way of doing things (camelCase)?

	return toCamelCase(text);

	return text
		.replace(/\s+/g, '-')
		.replace(/[^a-zA-Z0-9_-]+/g, '')
		.replace(/[-_]{2,}/g, (match) => match[0])
		.toLowerCase();
}
