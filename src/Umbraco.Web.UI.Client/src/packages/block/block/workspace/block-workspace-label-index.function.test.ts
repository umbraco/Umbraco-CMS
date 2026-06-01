import { resolveBlockWorkspaceLabelIndex } from './block-workspace-label-index.function.js';
import { expect } from '@open-wc/testing';

describe('resolveBlockWorkspaceLabelIndex', () => {
	describe('Existing block (cached index)', () => {
		it('returns the cached index when set', () => {
			expect(resolveBlockWorkspaceLabelIndex(2, undefined, 5)).to.equal(2);
		});

		it('returns 0 (a valid cached position) rather than falling through to originData', () => {
			expect(resolveBlockWorkspaceLabelIndex(0, { index: 7 }, 5)).to.equal(0);
		});

		it('prefers the cached index over a different originData index', () => {
			expect(resolveBlockWorkspaceLabelIndex(1, { index: 7 }, 5)).to.equal(1);
		});
	});

	describe('New block — explicit insertion index (originData.index >= 0)', () => {
		it('returns originData.index when no cached index is available', () => {
			expect(resolveBlockWorkspaceLabelIndex(undefined, { index: 3 }, 5)).to.equal(3);
		});

		it('returns 0 when originData.index is 0 (insert at start)', () => {
			expect(resolveBlockWorkspaceLabelIndex(undefined, { index: 0 }, 5)).to.equal(0);
		});
	});

	describe('New block — append sentinel (originData.index === -1)', () => {
		it('returns layoutsLength when -1 and the layouts length is known', () => {
			expect(resolveBlockWorkspaceLabelIndex(undefined, { index: -1 }, 5)).to.equal(5);
		});

		it('returns 0 when appending to an empty list', () => {
			expect(resolveBlockWorkspaceLabelIndex(undefined, { index: -1 }, 0)).to.equal(0);
		});

		it('returns undefined when layoutsLength is not yet known', () => {
			expect(resolveBlockWorkspaceLabelIndex(undefined, { index: -1 }, undefined)).to.equal(undefined);
		});
	});

	describe('No usable source — guards against NaN', () => {
		it('returns undefined when nothing is available', () => {
			expect(resolveBlockWorkspaceLabelIndex(undefined, undefined, undefined)).to.equal(undefined);
		});

		it('returns undefined when originData has no index property', () => {
			expect(resolveBlockWorkspaceLabelIndex(undefined, {} as never, 5)).to.equal(undefined);
		});

		it('returns undefined when originData.index is non-numeric', () => {
			expect(
				resolveBlockWorkspaceLabelIndex(undefined, { index: 'oops' } as unknown as { index: number }, 5),
			).to.equal(undefined);
		});
	});
});
