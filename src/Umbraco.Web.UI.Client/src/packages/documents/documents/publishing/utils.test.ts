import { expect } from '@open-wc/testing';
import { computeAncestorPublishedCultures, type UmbAncestorForCoverage } from './utils.js';
import { UmbDocumentVariantState } from '../variant-state.js';

const ancestor = (...variants: Array<{ culture: string | null; state: UmbDocumentVariantState }>): UmbAncestorForCoverage => ({
	variants,
});

describe('computeAncestorPublishedCultures', () => {
	it('returns undefined for an empty ancestor chain (root document)', () => {
		expect(computeAncestorPublishedCultures([])).to.equal(undefined);
	});

	it('returns [null] when the single ancestor is invariant-published', () => {
		const result = computeAncestorPublishedCultures([
			ancestor({ culture: null, state: UmbDocumentVariantState.PUBLISHED }),
		]);
		expect(result).to.deep.equal([null]);
	});

	it('returns [] when the single ancestor has no published variant', () => {
		const result = computeAncestorPublishedCultures([
			ancestor({ culture: null, state: UmbDocumentVariantState.DRAFT }),
		]);
		expect(result).to.deep.equal([]);
	});

	it('returns the culture for a single variant ancestor published in one culture', () => {
		const result = computeAncestorPublishedCultures([
			ancestor(
				{ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED },
				{ culture: 'da-DK', state: UmbDocumentVariantState.DRAFT },
			),
		]);
		expect(result).to.have.members(['en-US']);
		expect(result).to.have.lengthOf(1);
	});

	it('returns every published culture for a single variant ancestor', () => {
		const result = computeAncestorPublishedCultures([
			ancestor(
				{ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED },
				{ culture: 'da-DK', state: UmbDocumentVariantState.PUBLISHED },
			),
		]);
		expect(result).to.have.members(['en-US', 'da-DK']);
		expect(result).to.have.lengthOf(2);
	});

	it('counts PublishedPendingChanges as published', () => {
		const result = computeAncestorPublishedCultures([
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES }),
		]);
		expect(result).to.have.members(['en-US']);
	});

	it('excludes Draft and NotCreated variants', () => {
		const result = computeAncestorPublishedCultures([
			ancestor(
				{ culture: 'en-US', state: UmbDocumentVariantState.DRAFT },
				{ culture: 'da-DK', state: UmbDocumentVariantState.NOT_CREATED },
			),
		]);
		expect(result).to.deep.equal([]);
	});

	it('intersects published cultures across the chain', () => {
		// Grandparent publishes en-US, parent publishes en-US AND da-DK.
		// Only en-US is published in every ancestor.
		const result = computeAncestorPublishedCultures([
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED }),
			ancestor(
				{ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED },
				{ culture: 'da-DK', state: UmbDocumentVariantState.PUBLISHED },
			),
		]);
		expect(result).to.have.members(['en-US']);
		expect(result).to.have.lengthOf(1);
	});

	it('returns [] when the intersection is empty', () => {
		// Grandparent only publishes en-US, parent only publishes da-DK.
		const result = computeAncestorPublishedCultures([
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED }),
			ancestor({ culture: 'da-DK', state: UmbDocumentVariantState.PUBLISHED }),
		]);
		expect(result).to.deep.equal([]);
	});

	it('ignores invariant-published ancestors when intersecting (they add no constraint)', () => {
		// Grandparent is invariant-published; parent publishes only en-US.
		// Only en-US should be covered (parent constrains it).
		const result = computeAncestorPublishedCultures([
			ancestor({ culture: null, state: UmbDocumentVariantState.PUBLISHED }),
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED }),
		]);
		expect(result).to.have.members(['en-US']);
		expect(result).to.have.lengthOf(1);
	});

	it('returns [null] when every ancestor is invariant-published', () => {
		const result = computeAncestorPublishedCultures([
			ancestor({ culture: null, state: UmbDocumentVariantState.PUBLISHED }),
			ancestor({ culture: null, state: UmbDocumentVariantState.PUBLISHED }),
		]);
		expect(result).to.deep.equal([null]);
	});

	it('returns [] when any ancestor in the chain has nothing published', () => {
		// Middle ancestor has no published cultures — breaks the chain entirely.
		const result = computeAncestorPublishedCultures([
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED }),
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.DRAFT }),
			ancestor({ culture: 'en-US', state: UmbDocumentVariantState.PUBLISHED }),
		]);
		expect(result).to.deep.equal([]);
	});
});
