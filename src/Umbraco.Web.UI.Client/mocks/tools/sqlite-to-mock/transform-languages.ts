/**
 * Transform umbracoLanguage records into mock data format.
 */
import { prepare, writeDataFile, type Language } from './db.js';

export function transformLanguages(): void {
	// Query languages
	const query = prepare(`
		SELECT
			id, languageISOCode, languageCultureName, isDefaultVariantLang,
			mandatory, fallbackLanguageId
		FROM umbracoLanguage
		ORDER BY id
	`);

	const rows = query.all() as Language[];

	// Build fallback lookup map (languageId -> isoCode)
	const languageMap = new Map<number, string>();
	for (const row of rows) {
		languageMap.set(row.id, row.languageISOCode);
	}

	// Transform languages
	const languages = rows.map((row) => ({
		name: row.languageCultureName,
		isoCode: row.languageISOCode,
		isDefault: row.isDefaultVariantLang === 1,
		isMandatory: row.mandatory === 1,
		fallbackIsoCode: row.fallbackLanguageId ? languageMap.get(row.fallbackLanguageId) : undefined,
	}));

	// Generate TypeScript content
	const content = `import type { UmbMockLanguageModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockLanguageModel> = ${JSON.stringify(languages, null, '\t')};
`;

	writeDataFile('language.data.ts', content);
	console.log(`Transformed ${languages.length} languages`);
}
