import { clamp } from '@umbraco-cms/backoffice/external/uui';
export { clamp };

/**
 * Performs linear interpolation (lerp) between two numbers based on a blending factor.
 * @param {number} start - The starting value.
 * @param {number} end - The ending value.
 * @param {number} alpha - The blending factor, clamped to the range [0, 1].
 * @returns {number} The result of linear interpolation between `start` and `end` using `alpha`.
 * @example
 * // Interpolate between two values.
 * const value1 = 10;
 * const value2 = 20;
 * const alpha = 0.5; // Blend halfway between value1 and value2.
 * const result = lerp(value1, value2, alpha);
 * // result is 15
 *
 * // Ensure alpha is clamped to the range [0, 1].
 * const value3 = 5;
 * const value4 = 15;
 * const invalidAlpha = 1.5; // This will be clamped to 1.
 * const result2 = lerp(value3, value4, invalidAlpha);
 * // result2 is 15, equivalent to lerp(value3, value4, 1)
 */
export function lerp(start: number, end: number, alpha: number): number {
	// Ensure that alpha is clamped between 0 and 1
	alpha = clamp(alpha, 0, 1);

	// Perform linear interpolation
	return start * (1 - alpha) + end * alpha;
}

/**
 * Calculates the inverse linear interpolation (inverse lerp) factor based on a value between two numbers.
 * The inverse lerp factor indicates where the given `value` falls between `start` and `end`.
 *
 * If `value` is equal to `start`, the function returns 0. If `value` is equal to `end`, the function returns 1.
 * @param {number} start - The starting value.
 * @param {number} end - The ending value.
 * @param {number} value - The value to calculate the inverse lerp factor for.
 * @returns {number} The inverse lerp factor, a value in the range [0, 1], indicating where `value` falls between `start` and `end`.
 *   - If `start` and `end` are equal, the function returns 0.
 *   - If `value` is less than `start`, the factor is less than 0, indicating it's before `start`.
 *   - If `value` is greater than `end`, the factor is greater than 1, indicating it's after `end`.
 *   - If `value` is between `start` and `end`, the factor is between 0 and 1, indicating where `value` is along that range.
 * @example
 * // Calculate the inverse lerp factor for a value between two points.
 * const startValue = 10;
 * const endValue = 20;
 * const targetValue = 15; // The value we want to find the factor for.
 * const result = inverseLerp(startValue, endValue, targetValue);
 * // result is 0.5, indicating that targetValue is halfway between startValue and endValue.
 *
 * // Handle the case where start and end are equal.
 * const equalStartAndEnd = 5;
 * const result2 = inverseLerp(equalStartAndEnd, equalStartAndEnd, equalStartAndEnd);
 * // result2 is 0, as start and end are equal.
 */
export function inverseLerp(start: number, end: number, value: number): number {
	if (start === end) {
		return 0; // Avoid division by zero if start and end are equal
	}

	return (value - start) / (end - start);
}

/**
 * Calculates the absolute difference between two numbers.
 * @param {number} a - The first number.
 * @param {number} b - The second number.
 * @returns {number} The absolute difference between `a` and `b`.
 * @example
 * // Calculate the distance between two points on a number line.
 * const point1 = 5;
 * const point2 = 8;
 * const result = distance(point1, point2);
 * // result is 3
 *
 * // Calculate the absolute difference between two values.
 * const value1 = -10;
 * const value2 = 20;
 * const result2 = distance(value1, value2);
 * // result2 is 30
 */
export function distance(a: number, b: number): number {
	return Math.abs(a - b);
}

/**
 * Calculates the extrapolated final value based on an initial value and an increase factor.
 * @param {number} initialValue - The starting value.
 * @param {number} increaseFactor - The factor by which the value should increase
 *   (must be in the range [0(inclusive), 1(exclusive)] where 0 means no increase and 1 means no limit).
 * @returns {number} The extrapolated final value.
 *   Returns NaN if the increase factor is not within the valid range.
 * @example
 * // Valid input
 * const result = calculateExtrapolatedValue(100, 0.2);
 * // result is 125
 *
 * // Valid input
 * const result2 = calculateExtrapolatedValue(50, 0.5);
 * // result2 is 100
 *
 * // Invalid input (increaseFactor is out of range)
 * const result3 = calculateExtrapolatedValue(200, 1.2);
 * // result3 is NaN
 */
export function calculateExtrapolatedValue(initialValue: number, increaseFactor: number): number {
	if (increaseFactor < 0 || increaseFactor >= 1) {
		// Return a special value to indicate an invalid input.
		return NaN;
	}

	return initialValue / (1 - increaseFactor);
}

/**
 * Find the index for a target value on an array of individual values.
 * @param {number} target - The target value to interpolate to.
 * @param {Array<number>} weights - An array of values to interpolate between.
 * @returns
 */
export function getInterpolatedIndexOfPositionInWeightMap(target: number, weights: Array<number>): number {
	const map = [0];
	weights.reduce((a, b, i) => {
		return (map[i + 1] = a + b);
	}, 0);
	const foundValue = map.reduce((a, b) => {
		const aDiff = Math.abs(a - target);
		const bDiff = Math.abs(b - target);

		if (aDiff === bDiff) {
			return a < b ? a : b;
		} else {
			return bDiff < aDiff ? b : a;
		}
	});
	const foundIndex = map.indexOf(foundValue);
	const targetDiff = target - foundValue;
	let interpolatedIndex = foundIndex;
	if (targetDiff < 0 && foundIndex === 0) {
		// Don't adjust.
	} else if (targetDiff > 0 && foundIndex === map.length - 1) {
		// Don't adjust.
	} else {
		const foundInterpolationWeight = weights[targetDiff >= 0 ? foundIndex : foundIndex - 1];
		interpolatedIndex += foundInterpolationWeight === 0 ? interpolatedIndex : targetDiff / foundInterpolationWeight;
	}
	return interpolatedIndex;
}

/**
 * Combine the values of an array up to a certain index.
 * @param {number} index - The index to accumulate to, everything after this index will not be accumulated.
 * @param {Array<number>} weights - An array of values to accumulate.
 * @returns
 */
export function getAccumulatedValueOfIndex(index: number, weights: Array<number>): number {
	const len = Math.min(index, weights.length);
	let i = 0,
		calc = 0;
	while (i < len) {
		calc += weights[i++];
	}
	return calc;
}

/**
 *
 * @param {number} x - The x coordinate.
 * @param {number} y - The y coordinate.
 * @param {DOMRect} rect - The rectangle to check.
 * @param {number} expand - The amount to expand or contract the rectangle.
 * @returns
 */
export function isWithinRect(x: number, y: number, rect: DOMRect, expand = 0) {
	return x > rect.left - expand && x < rect.right + expand && y > rect.top - expand && y < rect.bottom + expand;
}
