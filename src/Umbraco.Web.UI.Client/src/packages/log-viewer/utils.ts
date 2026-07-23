/**
 * Converts a `YYYY-MM-DD` calendar date to the absolute (UTC) ISO 8601 timestamp for the very
 * start of that day (00:00:00.000) in the browser's local time zone.
 * @param {string} date A calendar date in `YYYY-MM-DD` format, as produced by `<umb-input-date type="date">`.
 * @returns {string} The ISO 8601 timestamp, or the original string if it cannot be parsed.
 */
export function umbGetStartOfDayInLocalTime(date: string): string {
	const parsed = parseLocalDate(date);
	if (!parsed) {
		return date;
	}

	return new Date(parsed.year, parsed.month - 1, parsed.day, 0, 0, 0, 0).toISOString();
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
	const parsed = parseLocalDate(date);
	if (!parsed) {
		return date;
	}

	return new Date(parsed.year, parsed.month - 1, parsed.day, 23, 59, 59, 999).toISOString();
}

/**
 * Parses a strict `YYYY-MM-DD` calendar date into its numeric parts.
 *
 * Returns `null` for anything that is not a real calendar date, including partial input
 * (`2023-08-`) and out-of-range values (`2023-13-40`, `2023-02-29`) that the `Date` constructor
 * would otherwise silently normalize to a different day.
 * @param {string} date A calendar date in `YYYY-MM-DD` format.
 * @returns {{ year: number; month: number; day: number } | null} The parsed parts, or `null` if invalid.
 */
function parseLocalDate(date: string): { year: number; month: number; day: number } | null {
	const parts = date.split('-');
	if (parts.length !== 3) {
		return null;
	}

	const [year, month, day] = parts.map(Number);
	if ([year, month, day].some((part) => !Number.isInteger(part))) {
		return null;
	}

	// Confirm the date round-trips: if the constructor normalized it (e.g. day 0 or month 13),
	// the read-back components will not match the input, so treat it as unparseable.
	const probe = new Date(year, month - 1, day);
	if (probe.getFullYear() !== year || probe.getMonth() !== month - 1 || probe.getDate() !== day) {
		return null;
	}

	return { year, month, day };
}
