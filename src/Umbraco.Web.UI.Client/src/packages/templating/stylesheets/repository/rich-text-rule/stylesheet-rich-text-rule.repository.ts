import { UmbStylesheetRichTextRuleServerDataSource } from './stylesheet-rich-text-rule.server.data-source.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import {
	ExtractRichTextStylesheetRulesRequestModel,
	InterpolateRichTextStylesheetRequestModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbStylesheetRichTextRuleRepository extends UmbRepositoryBase {
	#dataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbStylesheetRichTextRuleServerDataSource(host);
	}

	requestStylesheetRules(unique: string) {
		return this.#dataSource.getStylesheetRichTextRules(unique);
	}

	/**
	 * Existing content + array of rules => new content string
	 *
	 * @param {InterpolateRichTextStylesheetRequestModel} data
	 * @return {*}  {Promise<DataSourceResponse<InterpolateRichTextStylesheetResponseModel>>}
	 * @memberof UmbStylesheetRepository
	 */
	interpolateStylesheetRules(data: InterpolateRichTextStylesheetRequestModel) {
		return this.#dataSource.interpolateStylesheetRules(data);
	}

	/**
	 * content string => array of rules
	 *
	 * @param {ExtractRichTextStylesheetRulesRequestModel} data
	 * @return {*}
	 * @memberof UmbStylesheetRepository
	 */
	extractStylesheetRules(data: ExtractRichTextStylesheetRulesRequestModel) {
		return this.#dataSource.extractStylesheetRules(data);
	}
}
