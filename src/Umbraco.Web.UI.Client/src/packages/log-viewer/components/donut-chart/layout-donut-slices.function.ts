export interface UmbDonutSliceLayoutOptions {
	/** The smallest sweep, in degrees, that a drawable slice gets. Shrinks when there are too many slices for it. */
	minSweep: number;
	/** The angular gap, in degrees, between neighbouring slices. */
	gap: number;
}

export interface UmbDonutSliceLayout {
	/** The position of the slice in the amounts passed to the layout. */
	index: number;
	/** The slice's true share of the total, from 0 to 1. */
	share: number;
	/** Where the slice starts, in degrees clockwise from 12 o'clock. */
	startAngle: number;
	/** How far the slice extends, in degrees. */
	sweep: number;
}

const MAX_TOTAL_GAP_DEGREES = 60;

/**
 * Lays out the slices of a donut chart around a full circle.
 *
 * Amounts that are not positive finite numbers are left out. A single drawable slice is a full ring. Otherwise every
 * slice gets at least the minimum sweep, the sweep needed to lift small slices to it is taken from the larger slices in
 * proportion to how far they exceed it, and the sweeps plus the gaps between slices fill exactly 360 degrees. The
 * minimum shrinks so that inflated slices never take more than half of the ring.
 * @param {ReadonlyArray<number>} amounts - The amount of each slice, in display order.
 * @param {UmbDonutSliceLayoutOptions} options - The minimum sweep and the gap between slices.
 * @returns {Array<UmbDonutSliceLayout>} One entry per drawable slice, in input order.
 */
export function umbLayoutDonutSlices(
	amounts: ReadonlyArray<number>,
	options: UmbDonutSliceLayoutOptions,
): Array<UmbDonutSliceLayout> {
	const drawable = amounts
		.map((amount, index) => ({ index, amount: Number.isFinite(amount) && amount > 0 ? amount : 0 }))
		.filter((slice) => slice.amount > 0);

	if (drawable.length === 0) return [];
	if (drawable.length === 1) return [{ index: drawable[0].index, share: 1, startAngle: 0, sweep: 360 }];

	const largest = Math.max(...drawable.map((slice) => slice.amount));
	const weights = drawable.map((slice) => slice.amount / largest);
	const totalWeight = weights.reduce((sum, weight) => sum + weight, 0);

	const count = drawable.length;
	const gap = clampToRange(options.gap, 0, MAX_TOTAL_GAP_DEGREES / count);
	const available = 360 - gap * count;
	const minSweep = clampToRange(options.minSweep, 0, available / (2 * count));

	const idealSweeps = weights.map((weight) => (weight / totalWeight) * available);
	const totalExcess = idealSweeps.reduce((sum, sweep) => sum + Math.max(0, sweep - minSweep), 0);
	const excessScale = (available - minSweep * count) / totalExcess;

	let startAngle = gap / 2;
	return drawable.map((slice, i) => {
		const sweep = minSweep + Math.max(0, idealSweeps[i] - minSweep) * excessScale;
		const layout = { index: slice.index, share: weights[i] / totalWeight, startAngle, sweep };
		startAngle += sweep + gap;
		return layout;
	});
}

function clampToRange(value: number, min: number, max: number) {
	return Number.isFinite(value) ? Math.min(Math.max(value, min), max) : min;
}
