import type { DateTime } from '@umbraco-cms/backoffice/external/luxon';

export interface UmbTimeZone {
	value: string;
	name: string;
}

/**
 * Retrieves a list of supported time zones in the browser.
 * @param {Array<string>} [filter] - An optional array of time zone identifiers to filter the result on.
 * @returns {Array<UmbTimeZone>} An array of objects containing time zone values and names.
 */
export function getTimeZoneList(filter: Array<string> | undefined = undefined): Array<UmbTimeZone> {
	if (filter) {
		return filter.map((tz) => ({
			value: tz,
			name: getTimeZoneName(tz),
		}));
	}

	const timeZones = Intl.supportedValuesOf('timeZone')
		// Exclude offset time zones, e.g. 'Etc/GMT+2', as they are not consistent between browsers
		.filter((value) => value !== 'UTC' && !value.startsWith('Etc/'));

	// Add UTC to the top of the list
	timeZones.unshift('UTC');

	return timeZones.map((tz) => ({
		value: tz,
		name: getTimeZoneName(tz),
	}));
}

/**
 * Retrieves the client's time zone information.
 * @param {DateTime} [selectedDate] - An optional Luxon DateTime object to format the offset of the time zone.
 * @returns {UmbTimeZone} An object containing the client's time zone name and value.
 */
export function getClientTimeZone(): UmbTimeZone {
	const clientTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
	return {
		value: clientTimeZone,
		name: getTimeZoneName(clientTimeZone),
	};
}

/**
 * Returns the time zone offset for a given time zone ID and date.
 * @param timeZoneId - The time zone identifier (e.g., 'America/New_York').
 * @param date - The Luxon DateTime object for which to get the offset.
 * @returns {string} The time zone offset
 */
export function getTimeZoneOffset(timeZoneId: string, date: DateTime): string {
	return date.setZone(timeZoneId).toFormat('Z');
}

/**
 * Returns the browser's time zone name in a user-friendly format.
 * @param {string} timeZoneId - The time zone identifier.
 * @returns {string} A formatted time zone name.
 */
export function getTimeZoneName(timeZoneId: string) {
	if (timeZoneId === 'UTC') {
		return 'Coordinated Universal Time (UTC)';
	}

	return timeZoneId.replaceAll('_', ' ');
}

/**
 * Checks if two time zone identifiers are equivalent.
 * This function compares the resolved time zone names to determine if they are equivalent.
 * @param {string} tz1 - The first time zone identifier.
 * @param {string} tz2 - The second time zone identifier.
 * @returns {boolean} True if the time zones are equivalent, false otherwise.
 */
export function isEquivalentTimeZone(tz1: string, tz2: string): boolean {
	if (tz1 === tz2) {
		return true;
	}

	const tz1Name = new Intl.DateTimeFormat(undefined, { timeZone: tz1 }).resolvedOptions().timeZone;
	const tz2Name = new Intl.DateTimeFormat(undefined, { timeZone: tz2 }).resolvedOptions().timeZone;
	return tz1Name === tz2Name;
}
