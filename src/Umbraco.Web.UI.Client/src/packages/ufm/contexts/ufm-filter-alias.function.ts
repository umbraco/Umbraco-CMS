import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

const UMB_UFM_FILTER_KEBABCASE_MAP: Record<string, string> = {
	'strip-html': 'stripHtml',
	'title-case': 'titleCase',
	'word-limit': 'wordLimit',
};

const _reportedAliases = new Set<string>();

/**
 * Resolves a UFM filter alias, translating a deprecated kebab-case alias to its camelCase
 * equivalent (for UFMJS compatibility) and warning once per alias when a deprecated alias is used.
 * @param {string} alias The filter alias parsed from a UFM expression.
 * @returns {string} The camelCase alias for a deprecated kebab-case alias; otherwise the original alias.
 */
export function umbResolveUfmFilterAlias(alias: string): string {
	const mapped = UMB_UFM_FILTER_KEBABCASE_MAP[alias];
	if (!mapped) return alias;

	if (!_reportedAliases.has(alias)) {
		_reportedAliases.add(alias);
		new UmbDeprecation({
			deprecated: `The UFM filter alias "${alias}" is deprecated.`,
			removeInVersion: '20.0.0',
			solution: `Use the camelCase alias "${mapped}" instead.`,
		}).warn();
	}

	return mapped;
}
