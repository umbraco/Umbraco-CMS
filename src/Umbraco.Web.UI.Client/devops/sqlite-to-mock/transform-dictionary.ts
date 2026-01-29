/**
 * Transform cmsDictionary and cmsLanguageText records into mock data format.
 */
import { prepare, formatGuid, writeDataFile, type UmbracoNode, type Language } from './db.js';

interface DictionaryItem {
	pk: number;
	id: string;
	parent: string | null;
	key: string;
}

interface LanguageText {
	pk: number;
	languageId: number;
	UniqueId: string;
	value: string;
}

export function transformDictionary(): void {
	// Query dictionary items
	const dictionaryQuery = prepare(`
		SELECT pk, id, parent, key
		FROM cmsDictionary
		ORDER BY pk
	`);

	const dictionaryItems = dictionaryQuery.all() as DictionaryItem[];

	// Query language texts
	const textQuery = prepare(`
		SELECT pk, languageId, UniqueId, value
		FROM cmsLanguageText
	`);

	const languageTexts = textQuery.all() as LanguageText[];

	// Query languages for ISO code lookup
	const languageQuery = prepare(`
		SELECT id, languageISOCode
		FROM umbracoLanguage
	`);

	const languages = languageQuery.all() as { id: number; languageISOCode: string }[];
	const languageMap = new Map<number, string>();
	for (const lang of languages) {
		languageMap.set(lang.id, lang.languageISOCode);
	}

	// Group texts by dictionary ID
	const textsByDictionary = new Map<string, LanguageText[]>();
	for (const text of languageTexts) {
		const id = text.UniqueId.toLowerCase();
		const list = textsByDictionary.get(id) || [];
		list.push(text);
		textsByDictionary.set(id, list);
	}

	// Build parent lookup map (id -> formatted id)
	const idMap = new Map<string, string>();
	for (const item of dictionaryItems) {
		idMap.set(item.id.toLowerCase(), formatGuid(item.id));
	}

	// Check if items have children
	const hasChildrenMap = new Map<string, boolean>();
	for (const item of dictionaryItems) {
		if (item.parent) {
			hasChildrenMap.set(item.parent.toLowerCase(), true);
		}
	}

	// Transform dictionary items
	const dictionary = dictionaryItems.map((item) => {
		const itemId = formatGuid(item.id);
		const parentId = item.parent ? idMap.get(item.parent.toLowerCase()) : null;
		const texts = textsByDictionary.get(item.id.toLowerCase()) || [];

		// Build translations array
		const translations = texts.map((text) => ({
			isoCode: languageMap.get(text.languageId) || `unknown-${text.languageId}`,
			translation: text.value,
		}));

		return {
			id: itemId,
			parent: parentId ? { id: parentId } : null,
			name: item.key,
			hasChildren: hasChildrenMap.get(item.id.toLowerCase()) || false,
			translatedIsoCodes: translations.map((t) => t.isoCode),
			translations,
			flags: [] as string[],
		};
	});

	// Generate TypeScript content
	const content = `import type { UmbMockDictionaryModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockDictionaryModel> = ${JSON.stringify(dictionary, null, '\t')};
`;

	writeDataFile('dictionary.data.ts', content);
	console.log(`Transformed ${dictionary.length} dictionary items`);
}

// Run if called directly
transformDictionary();
