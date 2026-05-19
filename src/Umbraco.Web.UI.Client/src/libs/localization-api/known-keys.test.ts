import { expect } from '@open-wc/testing';
import en from '../../assets/lang/en.js';
import { UMB_KNOWN_LOCALIZATION_KEYS } from './known-keys.generated.js';

/**
 * Catches the "edited `en.ts` but forgot to regenerate" failure mode. The generated file is
 * committed (see `docs/package-development.md` → Type-safe localization keys), so without an
 * assertion like this a stale file would happily compile and only get caught the next time the
 * `prebuild` hook ran. By that point the PR may already be merged.
 */
describe('UmbKnownLocalizationSet generation', () => {
	it('stays in sync with `assets/lang/en.ts`', () => {
		const expected = new Set<string>();
		for (const [group, entries] of Object.entries(en)) {
			if (typeof entries !== 'object' || entries === null) continue;
			for (const key of Object.keys(entries)) {
				expected.add(`${group}_${key}`);
			}
		}

		const actual = new Set<string>(UMB_KNOWN_LOCALIZATION_KEYS);

		const missing = [...expected].filter((k) => !actual.has(k));
		const extra = [...actual].filter((k) => !expected.has(k));

		const regenHint = "Run 'npm run generate:localization-keys' to regenerate `known-keys.generated.ts`.";

		expect(
			missing,
			`${missing.length} key(s) in en.ts are missing from the generated file. ${regenHint}\nMissing: ${missing.join(', ')}`,
		).to.have.length(0);

		expect(
			extra,
			`${extra.length} key(s) in the generated file no longer exist in en.ts. ${regenHint}\nStale: ${extra.join(', ')}`,
		).to.have.length(0);
	});
});
