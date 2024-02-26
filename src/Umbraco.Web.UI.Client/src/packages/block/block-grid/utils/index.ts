export function closestColumnSpanOption(target: number, map: Array<number>, max: number) {
	if (map.length > 0) {
		const result = map.reduce((a, b) => {
			if (a > max) {
				return b;
			}
			const aDiff = Math.abs(a - target);
			const bDiff = Math.abs(b - target);

			if (aDiff === bDiff) {
				return a < b ? a : b;
			} else {
				return bDiff < aDiff ? b : a;
			}
		});
		if (result) {
			return result;
		}
	}
	return;
}
