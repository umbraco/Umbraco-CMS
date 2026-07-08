import { expect } from '@open-wc/testing';
import type { UmbEntityVariantOptionModel } from './types.js';
import { sortVariants } from './sort-variants.function.js';
import { PublishableVariantStateModel as State } from '@umbraco-cms/backoffice/external/backend-api';

function makeVariant(
	name: string,
	opts: { isDefault?: boolean; isMandatory?: boolean; state?: string | null } = {},
): UmbEntityVariantOptionModel {
	return {
		unique: name,
		culture: null,
		segment: null,
		language: {
			entityType: 'language',
			unique: name,
			name,
			isDefault: opts.isDefault ?? false,
			isMandatory: opts.isMandatory ?? false,
			fallbackIsoCode: null,
		} as UmbEntityVariantOptionModel['language'],
		...(opts.state !== undefined && {
			variant: {
				name,
				culture: null,
				segment: null,
				createDate: null,
				updateDate: null,
				state: opts.state ?? null,
				flags: [],
			},
		}),
	};
}

describe('sortVariants', () => {
	it('sorts the default language first', () => {
		const result = [
			makeVariant('Danish', { state: State.PUBLISHED }),
			makeVariant('English', { isDefault: true, state: State.PUBLISHED }),
		].sort(sortVariants);

		expect(result.map((v) => v.language.name)).to.deep.equal(['English', 'Danish']);
	});

	it('sorts mandatory (non-published) variants above non-mandatory variants', () => {
		const result = [
			makeVariant('German', { state: State.DRAFT }),
			makeVariant('Danish', { isMandatory: true, state: State.DRAFT }),
		].sort(sortVariants);

		expect(result.map((v) => v.language.name)).to.deep.equal(['Danish', 'German']);
	});

	it('does not apply mandatory boost when the variant is already published', () => {
		const result = [
			makeVariant('German', { isMandatory: true, state: State.PUBLISHED }),
			makeVariant('Danish', { isMandatory: true, state: State.PUBLISHED }),
			makeVariant('French', { isMandatory: true, state: State.PUBLISHED }),
		].sort(sortVariants);

		// All published-mandatory — falls through to alphabetical
		expect(result.map((v) => v.language.name)).to.deep.equal(['Danish', 'French', 'German']);
	});

	it('sorts by publish state: published → draft → not-created → trashed', () => {
		const result = [
			makeVariant('D', { state: State.TRASHED }),
			makeVariant('A', { state: State.NOT_CREATED }),
			makeVariant('B', { state: State.DRAFT }),
			makeVariant('C', { state: State.PUBLISHED }),
		].sort(sortVariants);

		expect(result.map((v) => v.language.name)).to.deep.equal(['C', 'B', 'A', 'D']);
	});

	it('sorts alphabetically by language name when all other criteria are equal', () => {
		const result = [
			makeVariant('Swedish', { state: State.DRAFT }),
			makeVariant('Danish', { state: State.DRAFT }),
			makeVariant('French', { state: State.DRAFT }),
		].sort(sortVariants);

		expect(result.map((v) => v.language.name)).to.deep.equal(['Danish', 'French', 'Swedish']);
	});

	it('applies all criteria in priority order', () => {
		const result = [
			makeVariant('French', { state: State.PUBLISHED }),
			makeVariant('Danish', { isMandatory: true, state: State.DRAFT }),
			makeVariant('German', { state: State.DRAFT }),
			makeVariant('English', { isDefault: true, state: State.PUBLISHED }),
		].sort(sortVariants);

		expect(result.map((v) => v.language.name)).to.deep.equal([
			'English', // default language → rank 1
			'Danish', // mandatory non-published → rank 2
			'French', // non-mandatory, published (state=1) → rank 3
			'German', // non-mandatory, draft (state=2) → rank 4
		]);
	});
});
