import { RichTextRuleModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbStylesheetRuleManager {
	#umbRuleRegex = /\/\*\*\s*umb_name:\s*(?<name>[^*\r\n]*?)\s*\*\/\s*(?<selector>[^,{]*?)\s*{\s*(?<styles>.*?)\s*}/gis;

	/**
	 * Extracts umbraco rules from a stylesheet content
	 * @param {string} stylesheetContent
	 * @return {*}
	 * @memberof UmbStylesheetRuleManager
	 */
	extractRules(stylesheetContent: string) {
		const regex = this.#umbRuleRegex;
		if (!stylesheetContent) throw Error('No Stylesheet content');
		return [...stylesheetContent.matchAll(regex)].map((match) => match.groups);
	}

	/**
	 * Inserts umbraco rules into stylesheet content
	 * @param {string} stylesheetContent
	 * @param {UmbStylesheetRule[]} rules
	 * @return {*}
	 * @memberof UmbStylesheetRuleManager
	 */
	insertRules(stylesheetContent: string, rules: Array<RichTextRuleModel>) {
		const regex = this.#umbRuleRegex;
		if (!stylesheetContent) throw Error('No Stylesheet content');
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
	${rule.styles} 
}
`,
				)
				.join('')}`;
		return newContent;
	}
}
