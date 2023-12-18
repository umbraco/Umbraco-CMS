import { UmbServerPathUniqueSerializer } from '../../../utils/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	ExtractRichTextStylesheetRulesRequestModel,
	InterpolateRichTextStylesheetRequestModel,
	StylesheetResource,
} from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Stylesheet rich text rules that fetches data from the server
 * @export
 * @class UmbStylesheetRichTextRuleServerDataSource
 */
export class UmbStylesheetRichTextRuleServerDataSource {
	#host: UmbControllerHost;
	#serverPathUniqueSerializer = new UmbServerPathUniqueSerializer();

	/**
	 * Creates an instance of UmbStylesheetRichTextRuleServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetRichTextRuleServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get's the rich text rules for a stylesheet
	 *
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbStylesheetRichTextRuleServerDataSource
	 */
	getStylesheetRichTextRules(unique: string) {
		const path = this.#serverPathUniqueSerializer.toServerPath(unique);
		return tryExecuteAndNotify(this.#host, StylesheetResource.getStylesheetRichTextRules({ path }));
	}

	/**
	 * Extracts the rich text rules from a stylesheet string. In simple words: takes a stylesheet string and returns a array of rules.
	 *
	 * @param {ExtractRichTextStylesheetRulesRequestModel} data
	 * @return {*}
	 * @memberof UmbStylesheetRichTextRuleServerDataSource
	 */
	extractStylesheetRules(data: ExtractRichTextStylesheetRulesRequestModel) {
		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.postStylesheetRichTextExtractRules({ requestBody: data }),
		);
	}

	/**
	 * Interpolates the rich text rules from a stylesheet string. In simple words: takes a array of rules and existing content. Returns new content with the rules applied.
	 *
	 * @param {InterpolateRichTextStylesheetRequestModel} data
	 * @return {*}
	 * @memberof UmbStylesheetRichTextRuleServerDataSource
	 */
	interpolateStylesheetRules(data: InterpolateRichTextStylesheetRequestModel) {
		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.postStylesheetRichTextInterpolateRules({ requestBody: data }),
		);
	}
}
