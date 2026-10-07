export const UMB_EXAMPLES_STORAGE_KEY = 'umb:examples';

export function getSelectedExampleNames(): Array<string> {
	let names = (import.meta.env.VITE_EXAMPLES ?? '')
		.split(',')
		.map((name) => name.trim())
		.filter(Boolean);

	try {
		const stored = JSON.parse(localStorage.getItem(UMB_EXAMPLES_STORAGE_KEY) ?? 'null');
		if (Array.isArray(stored) && stored.every((name) => typeof name === 'string')) {
			names = stored;
		}
	} catch {
		// Malformed JSON is treated as unset.
	}

	return names;
}
