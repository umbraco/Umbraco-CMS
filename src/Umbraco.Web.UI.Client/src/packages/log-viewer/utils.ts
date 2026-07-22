/**
 * Converts a `YYYY-MM-DD` calendar date to the absolute (UTC) ISO 8601 timestamp for the very
 * start of that day (00:00:00.000) in the browser's local time zone.
 * @param {string} date A calendar date in `YYYY-MM-DD` format, as produced by `<umb-input-date type="date">`.
 * @returns {string} The ISO 8601 timestamp, or the original string if it cannot be parsed.
 */
export function umbGetStartOfDayInLocalTime(date: string): string {
	return toLocalInstant(date, 0, 0, 0, 0);
}

/**
 * Converts a `YYYY-MM-DD` calendar date to the absolute (UTC) ISO 8601 timestamp for the very
 * end of that day (23:59:59.999) in the browser's local time zone.
 *
 * The log viewer stores entries in files bucketed by the *server's* calendar date. Sending an
 * absolute instant (rather than a bare date) lets the server resolve the correct files regardless
 * of the offset between the browser and the server, so entries logged late in the user's local day
 * are not missed when the server has already rolled over to the next date (#14710).
 * @param {string} date A calendar date in `YYYY-MM-DD` format, as produced by `<umb-input-date type="date">`.
 * @returns {string} The ISO 8601 timestamp, or the original string if it cannot be parsed.
 */
export function umbGetEndOfDayInLocalTime(date: string): string {
	return toLocalInstant(date, 23, 59, 59, 999);
}

/**
 * Builds an absolute (UTC) ISO 8601 timestamp for a `YYYY-MM-DD` date at a specific local time of day.
 * @param {string} date A calendar date in `YYYY-MM-DD` format.
 * @param {number} hours The local hours component.
 * @param {number} minutes The local minutes component.
 * @param {number} seconds The local seconds component.
 * @param {number} milliseconds The local milliseconds component.
 * @returns {string} The ISO 8601 timestamp, or the original string if it cannot be parsed.
 */
function toLocalInstant(date: string, hours: number, minutes: number, seconds: number, milliseconds: number): string {
	const parts = date.split('-');
	if (parts.length !== 3) {
		return date;
	}

	const [year, month, day] = parts.map(Number);
	if ([year, month, day].some((part) => !Number.isFinite(part))) {
		return date;
	}

	const instant = new Date(year, month - 1, day, hours, minutes, seconds, milliseconds);
	if (Number.isNaN(instant.getTime())) {
		return date;
	}

	return instant.toISOString();
}
