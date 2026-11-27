import { manifests } from './manifests.js';
import { expect } from '@open-wc/testing';

type LocaleWithTextInfo = Intl.Locale & { getTextInfo(): { direction: 'ltr' | 'rtl' } };

const directionOf = (culture: string) => (new Intl.Locale(culture) as LocaleWithTextInfo).getTextInfo().direction;

describe('Core localization manifests', () => {
	it('should declare the direction each language is written in', () => {
		const mismatched = manifests
			.filter((manifest) => (manifest.meta.direction ?? 'ltr') !== directionOf(manifest.meta.culture))
			.map((manifest) => manifest.meta.culture);

		expect(mismatched).to.be.empty;
	});
});
