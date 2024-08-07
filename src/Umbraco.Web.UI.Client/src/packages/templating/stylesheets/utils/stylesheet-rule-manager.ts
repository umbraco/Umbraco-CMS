import type { UmbStylesheetRule } from '../types.js';

export class UmbStylesheetRuleManager {
	#umbRuleRegex = /\/\*\*\s*umb_name:\s*(?<name>[^*\r\n]*?)\s*\*\/\s*(?<selector>[^,{]*?)\s*{\s*(?<styles>.*?)\s*}/gis;

	/**
	 * Extracts umbraco rules from a stylesheet content
	 * @param {string} stylesheetContent
	 * @returns {*}
	 * @memberof UmbStylesheetRuleManager
	 */
	extractRules(stylesheetContent: string): Array<UmbStylesheetRule> {
		const regex = this.#umbRuleRegex;
		if (!stylesheetContent) throw Error('No Stylesheet content');
		return [...stylesheetContent.matchAll(regex)].map((match) => {
			const rule: UmbStylesheetRule = {
				name: match.groups?.name || '',
				selector: match.groups?.selector || '',
				styles: match.groups?.styles || '',
			};
			return rule;
		});
	}

	/**
	 * Inserts umbraco rules into stylesheet content
	 * @param {string} stylesheetContent
	 * @param {UmbStylesheetRule[]} rules
	 * @returns {*}
	 * @memberof UmbStylesheetRuleManager
	 */
	insertRules(stylesheetContent: string, rules: Array<any>): string {
		const regex = this.#umbRuleRegex;
		if (!stylesheetContent && !rules) return '';

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		//@ts-ignore
		const cleanedContent = stylesheetContent?.replaceAll(regex, '');
		const newContent = `
      ${cleanedContent.replace(/[\r\n]+$/, '')}
      ${rules
				?.map(
					(rule) => `
/**umb_name:${rule.name}*/
${rule.selector} {
	${rule.styles ?? ''}
}
`,
				)
				.join('')}`;
		return newContent.trim();
	}
}
