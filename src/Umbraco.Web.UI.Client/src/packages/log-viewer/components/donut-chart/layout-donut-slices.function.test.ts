import { umbLayoutDonutSlices } from './layout-donut-slices.function.js';
import type { UmbDonutSliceLayout, UmbDonutSliceLayoutOptions } from './layout-donut-slices.function.js';
import { expect } from '@open-wc/testing';

const OPTIONS: UmbDonutSliceLayoutOptions = { minSweep: 3.6, gap: 1.5 };
const EPSILON = 1e-9;

function sweepOf(layout: Array<UmbDonutSliceLayout>, index: number): number {
	const slice = layout.find((entry) => entry.index === index);
	if (!slice) throw new Error(`No slice laid out for index ${index}`);
	return slice.sweep;
}

function gapsOf(layout: Array<UmbDonutSliceLayout>): Array<number> {
	return layout.map((slice, i) => {
		const next = layout[(i + 1) % layout.length];
		const nextStart = i === layout.length - 1 ? next.startAngle + 360 : next.startAngle;
		return nextStart - (slice.startAngle + slice.sweep);
	});
}

function seededRandom(seed: number): () => number {
	let state = seed;
	return () => {
		state = (state * 1664525 + 1013904223) % 4294967296;
		return state / 4294967296;
	};
}

function skewedAmounts(count: number, random: () => number): Array<number> {
	return Array.from({ length: count }, () => Math.round(10 ** (random() * 6)));
}

function expectSweepsToFollowAmounts(amounts: Array<number>, layout: Array<UmbDonutSliceLayout>): void {
	const byAmount = [...layout].sort((a, b) => amounts[a.index] - amounts[b.index]);
	byAmount.slice(1).forEach((slice, i) => {
		const previous = byAmount[i];
		const message = `amounts ${amounts.join(', ')}`;
		if (amounts[slice.index] === amounts[previous.index]) {
			expect(slice.sweep, message).to.be.closeTo(previous.sweep, EPSILON);
		} else {
			expect(slice.sweep, message).to.be.at.least(previous.sweep - EPSILON);
		}
	});
}

describe('umbLayoutDonutSlices', () => {
	describe('drawable slices', () => {
		it('treats negative, NaN and non-finite amounts as zero and does not lay them out', () => {
			const layout = umbLayoutDonutSlices([-5, NaN, Infinity, -Infinity, 0, 10, 30], OPTIONS);

			expect(layout.map((slice) => slice.index)).to.deep.equal([5, 6]);
			expect(layout[0].share).to.be.closeTo(0.25, EPSILON);
			expect(layout[1].share).to.be.closeTo(0.75, EPSILON);
		});

		it('lays out nothing when no amount is drawable', () => {
			expect(umbLayoutDonutSlices([], OPTIONS)).to.have.lengthOf(0);
			expect(umbLayoutDonutSlices([0, 0, -1, NaN], OPTIONS)).to.have.lengthOf(0);
		});

		it('lays out a single drawable slice as a full ring without a gap', () => {
			const layout = umbLayoutDonutSlices([0, 42, 0], OPTIONS);

			expect(layout).to.deep.equal([{ index: 1, share: 1, startAngle: 0, sweep: 360 }]);
		});

		it('handles amounts whose sum is larger than the largest finite number', () => {
			const layout = umbLayoutDonutSlices([Number.MAX_VALUE, Number.MAX_VALUE], OPTIONS);

			expect(layout[0].share).to.equal(0.5);
			expect(layout[0].sweep).to.equal(layout[1].sweep);
			expect(layout[0].sweep).to.be.greaterThan(0);
		});
	});

	describe('filling the circle', () => {
		it('makes the sweeps and gaps add up to exactly 360 degrees, with equal gaps', () => {
			const random = seededRandom(1);
			for (let count = 2; count <= 30; count++) {
				const layout = umbLayoutDonutSlices(skewedAmounts(count, random), OPTIONS);
				const gaps = gapsOf(layout);
				const total = layout.reduce((sum, slice) => sum + slice.sweep, 0) + gaps.reduce((sum, gap) => sum + gap, 0);

				expect(total, `${count} slices`).to.be.closeTo(360, EPSILON);
				gaps.forEach((gap) => expect(gap, `${count} slices`).to.be.closeTo(OPTIONS.gap, EPSILON));
			}
		});

		it('never overlaps slices or produces non-finite or empty sweeps, from 1 to 30 slices', () => {
			const random = seededRandom(2);
			for (let count = 1; count <= 30; count++) {
				const layout = umbLayoutDonutSlices(skewedAmounts(count, random), OPTIONS);

				expect(layout, `${count} slices`).to.have.lengthOf(count);
				layout.forEach((slice, i) => {
					expect(Number.isFinite(slice.startAngle) && Number.isFinite(slice.sweep), `${count} slices`).to.be.true;
					expect(slice.sweep, `${count} slices`).to.be.greaterThan(0);
					if (i > 0) {
						const previous = layout[i - 1];
						expect(slice.startAngle, `${count} slices`).to.be.at.least(previous.startAngle + previous.sweep - EPSILON);
					}
				});
				const last = layout[layout.length - 1];
				expect(last.startAngle + last.sweep, `${count} slices`).to.be.at.most(360 + layout[0].startAngle + EPSILON);
			}
		});
	});

	describe('minimum sweep', () => {
		it('gives every drawable slice at least the minimum sweep', () => {
			const layout = umbLayoutDonutSlices([1_000_000, 1, 1, 1, 0.001], OPTIONS);

			layout.forEach((slice) => expect(slice.sweep).to.be.at.least(OPTIONS.minSweep - EPSILON));
		});

		it('shrinks the minimum when there are too many slices for each to get it, so all slices still fit', () => {
			const amounts = [1_000_000, ...Array.from({ length: 199 }, () => 1)];
			const layout = umbLayoutDonutSlices(amounts, OPTIONS);
			const total = layout.reduce((sum, slice) => sum + slice.sweep, 0) + gapsOf(layout).reduce((a, b) => a + b, 0);

			expect(layout).to.have.lengthOf(200);
			expect(total).to.be.closeTo(360, EPSILON);
			layout.forEach((slice) => expect(slice.sweep).to.be.greaterThan(0));
			expect(sweepOf(layout, 0)).to.be.greaterThan(sweepOf(layout, 1) * 10);
		});
	});

	describe('proportions', () => {
		it('never draws a larger amount smaller than a smaller amount', () => {
			const random = seededRandom(3);
			for (let run = 0; run < 200; run++) {
				const amounts = skewedAmounts(2 + Math.floor(random() * 29), random);
				expectSweepsToFollowAmounts(amounts, umbLayoutDonutSlices(amounts, OPTIONS));
			}
		});

		it('reports the true share of each slice, regardless of the drawn sweep', () => {
			const amounts = [50000, 1200, 30, 2, 1];
			const layout = umbLayoutDonutSlices(amounts, OPTIONS);

			layout.forEach((slice) => expect(slice.share).to.be.closeTo(amounts[slice.index] / 51233, EPSILON));
		});

		it('inflates small slices only to the minimum, without shrinking other small slices', () => {
			const layout = umbLayoutDonutSlices([90, ...Array.from({ length: 25 }, () => 0.4)], OPTIONS);

			layout.slice(1).forEach((slice) => expect(slice.sweep).to.be.closeTo(OPTIONS.minSweep, EPSILON));
		});

		it('takes the sweep for inflated slices from larger slices in proportion to their excess over the minimum', () => {
			const amounts = [600, 300, 100, 1, 1];
			const layout = umbLayoutDonutSlices(amounts, OPTIONS);
			const available = 360 - OPTIONS.gap * amounts.length;
			const ideal = (index: number) => (amounts[index] / 1002) * available;
			const excessRatio = (a: number, b: number) =>
				(sweepOf(layout, a) - OPTIONS.minSweep) / (sweepOf(layout, b) - OPTIONS.minSweep);

			expect(excessRatio(0, 1)).to.be.closeTo((ideal(0) - OPTIONS.minSweep) / (ideal(1) - OPTIONS.minSweep), EPSILON);
			expect(excessRatio(1, 2)).to.be.closeTo((ideal(1) - OPTIONS.minSweep) / (ideal(2) - OPTIONS.minSweep), EPSILON);
			[0, 1, 2].forEach((index) => expect(sweepOf(layout, index)).to.be.below(ideal(index)));
		});

		it('keeps a dominant slice close to its true share when several tiny slices are inflated', () => {
			const layout = umbLayoutDonutSlices([50000, 1200, 30, 2, 1], OPTIONS);
			const drawnShare = sweepOf(layout, 0) / 360;

			expect(layout[0].share).to.be.closeTo(0.976, 0.001);
			expect(drawnShare).to.be.greaterThan(0.92);
			expect(sweepOf(layout, 1)).to.be.greaterThan(sweepOf(layout, 4));
		});

		it('keeps a large slice the largest by far when many small slices are inflated', () => {
			const layout = umbLayoutDonutSlices([90, ...Array.from({ length: 25 }, () => 0.4)], OPTIONS);
			const drawnShare = sweepOf(layout, 0) / 360;

			expect(drawnShare).to.be.greaterThan(0.6);
			layout.slice(1).forEach((slice) => expect(sweepOf(layout, 0)).to.be.greaterThan(slice.sweep * 50));
		});
	});
});
