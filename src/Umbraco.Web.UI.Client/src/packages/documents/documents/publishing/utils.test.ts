import { expect } from '@open-wc/testing';
import { computeAncestorPublishCoverage, type UmbAncestorForCoverage } from './utils.js';
import { UmbDocumentVariantState } from '../variant-state.js';

const ancestor = (
	...variants: Array<{ culture: string | null; state: UmbDocumentVariantState }>
): UmbAncestorForCoverage => ({
	variants,
});

describe('computeAncestorPublishCoverage', () => {
	it('returns undefined for an empty ancestor chain (root document)', () => {
		expect(computeAncestorPublishCoverage([])).to.equal(undefined);
	});

	it('reports the path published with no culture constraint when every ancestor is invariant-published', () => {
		const result = computeAncestorPublishCoverage([
			ancestor({ culture: null, state: UmbDocumentVariantState.PUBLISHED }),
			ancestor({ culture: null, state: UmbDocumentVariantState.PUBLISHED }),
		]);
		expect(result).to.deep.equal({ isPathPublished: true, publishedCultures: null });
	});

	it('reports the path unpublished when an invariant ancestor is not published', () => {
		const result = computeAncestorPublishCoverage([
			ancestor({ culture: null, state: UmbDocumentVariantState.PUBLISHED }),
			ancestor({ culture: null, state: UmbDocumentVariantState.DRAFT }),
		]);
		expect(result?.isPathPublished).to.equal(false);
	});

	it('reports the path unpublished when a variant ancestor is published in no culture', () => {
		const result = computeAncestorPublishCoverage([
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED }),
			ancestor(
				{ culture: 'en-US', state: UmbDocumentVariantState.DRAFT },
				{ culture: 'da-DK', state: UmbDocumentVariantState.NOT_CREATED },
			),
		]);
		expect(result).to.deep.equal({ isPathPublished: false, publishedCultures: [] });
	});

	it('reports the path published when every ancestor is published in some culture, even with no culture in common', () => {
		const result = computeAncestorPublishCoverage([
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED }),
			ancestor({ culture: 'da-DK', state: UmbDocumentVariantState.PUBLISHED }),
		]);
		expect(result).to.deep.equal({ isPathPublished: true, publishedCultures: [] });
	});

	it('counts PublishedPendingChanges as published', () => {
		const result = computeAncestorPublishCoverage([
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES }),
		]);
		expect(result).to.deep.equal({ isPathPublished: true, publishedCultures: ['en-US'] });
	});

	it('covers only the cultures published in every ancestor', () => {
		const result = computeAncestorPublishCoverage([
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED }),
			ancestor(
				{ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED },
				{ culture: 'da-DK', state: UmbDocumentVariantState.PUBLISHED },
			),
		]);
		expect(result).to.deep.equal({ isPathPublished: true, publishedCultures: ['en-US'] });
	});

	it('adds no culture constraint for an invariant-published ancestor', () => {
		const result = computeAncestorPublishCoverage([
			ancestor({ culture: null, state: UmbDocumentVariantState.PUBLISHED }),
			ancestor(
				{ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED },
				{ culture: 'da-DK', state: UmbDocumentVariantState.DRAFT },
			),
		]);
		expect(result).to.deep.equal({ isPathPublished: true, publishedCultures: ['en-US'] });
	});
});
