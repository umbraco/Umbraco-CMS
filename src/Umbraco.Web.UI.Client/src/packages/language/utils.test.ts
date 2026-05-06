import { sortLanguages } from './utils.js';
import type { UmbLanguageDetailModel } from './types.js';
import { UMB_LANGUAGE_ENTITY_TYPE } from './entity.js';
import { expect } from '@open-wc/testing';

const makeLanguage = (overrides: Partial<UmbLanguageDetailModel>): UmbLanguageDetailModel => ({
	entityType: UMB_LANGUAGE_ENTITY_TYPE,
	unique: overrides.unique ?? overrides.name ?? 'lang',
	name: overrides.name ?? '',
	isDefault: false,
	isMandatory: false,
	fallbackIsoCode: null,
	...overrides,
});

describe('sortLanguages', () => {
	it('sorts the default language first regardless of input order', () => {
		const english = makeLanguage({ unique: 'en', name: 'English', isDefault: true });
		const french = makeLanguage({ unique: 'fr', name: 'French' });
		const danish = makeLanguage({ unique: 'da', name: 'Danish' });

		const sorted = [danish, french, english].sort(sortLanguages);

		expect(sorted.map((l) => l.unique)).to.eql(['en', 'da', 'fr']);
	});

	it('sorts mandatory languages above non-mandatory (when no default involved)', () => {
		const french = makeLanguage({ unique: 'fr', name: 'French' });
		const danish = makeLanguage({ unique: 'da', name: 'Danish', isMandatory: true });
		const german = makeLanguage({ unique: 'de', name: 'German' });

		const sorted = [french, german, danish].sort(sortLanguages);

		expect(sorted.map((l) => l.unique)).to.eql(['da', 'fr', 'de']);
	});

	it('sorts equal-priority languages alphabetically by name', () => {
		const spanish = makeLanguage({ unique: 'es', name: 'Spanish' });
		const french = makeLanguage({ unique: 'fr', name: 'French' });
		const german = makeLanguage({ unique: 'de', name: 'German' });

		const sorted = [spanish, german, french].sort(sortLanguages);

		expect(sorted.map((l) => l.name)).to.eql(['French', 'German', 'Spanish']);
	});

	it('applies the full priority chain: default → mandatory → name', () => {
		const english = makeLanguage({ unique: 'en', name: 'English', isDefault: true });
		const danish = makeLanguage({ unique: 'da', name: 'Danish', isMandatory: true });
		const french = makeLanguage({ unique: 'fr', name: 'French' });
		const german = makeLanguage({ unique: 'de', name: 'German' });
		const spanish = makeLanguage({ unique: 'es', name: 'Spanish' });

		const sorted = [spanish, french, english, german, danish].sort(sortLanguages);

		expect(sorted.map((l) => l.unique)).to.eql(['en', 'da', 'fr', 'de', 'es']);
	});

	it('places defined names before missing names', () => {
		const french = makeLanguage({ unique: 'fr', name: 'French' });
		const empty = makeLanguage({ unique: 'xx', name: '' });

		const sorted = [empty, french].sort(sortLanguages);

		expect(sorted.map((l) => l.unique)).to.eql(['fr', 'xx']);
	});
});
