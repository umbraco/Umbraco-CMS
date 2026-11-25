import type { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

const SHARE_FORMAT: Intl.NumberFormatOptions = { style: 'percent', maximumFractionDigits: 1 };

/**
 * Formats a share of a total as a localized percentage with at most one decimal, without rounding a non-zero share
 * to 0% or a partial share to 100%: those read as "<0.1%" and ">99.9%".
 * @param {number} share - The share, from 0 to 1.
 * @param {Pick<UmbLocalizationController, 'number'>} localize - The localization controller used to format numbers.
 * @returns {string} The formatted percentage.
 */
export function umbFormatDonutShare(share: number, localize: Pick<UmbLocalizationController, 'number'>): string {
	const formatted = localize.number(share, SHARE_FORMAT);
	if (share > 0 && formatted === localize.number(0, SHARE_FORMAT)) return `<${localize.number(0.001, SHARE_FORMAT)}`;
	if (share < 1 && formatted === localize.number(1, SHARE_FORMAT)) return `>${localize.number(0.999, SHARE_FORMAT)}`;
	return formatted;
}
