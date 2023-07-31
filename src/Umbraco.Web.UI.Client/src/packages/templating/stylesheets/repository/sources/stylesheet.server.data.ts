import type { StylesheetDetails } from '../../index.js';
import { DataSourceResponse, UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	CreateStylesheetRequestModel,
	ExtractRichTextStylesheetRulesRequestModel,
	ExtractRichTextStylesheetRulesResponseModel,
	InterpolateRichTextStylesheetRequestModel,
	InterpolateRichTextStylesheetResponseModel,
	RichTextStylesheetRulesResponseModel,
	StylesheetResource,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Stylesheet that fetches data from the server
 * @export
 * @class UmbStylesheetServerDataSource
 * @implements {UmbStylesheetServerDataSource}
 */
export class UmbStylesheetServerDataSource
	implements UmbDataSource<CreateStylesheetRequestModel, string, UpdateStylesheetRequestModel, StylesheetDetails>
{
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbStylesheetServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbStylesheetServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}
	createScaffold(parentId: string | null): Promise<DataSourceResponse<StylesheetDetails>> {
		throw new Error('Method not implemented.');
	}

	/**
	 * Fetches a Stylesheet with the given path from the server
	 * @param {string} path
	 * @return {*}
	 * @memberof UmbStylesheetServerDataSource
	 */
	async get(path: string) {
		if (!path) throw new Error('Path is missing');
		return tryExecuteAndNotify(this.#host, StylesheetResource.getStylesheet({ path }));
	}
	/**
	 * Creates a new Stylesheet
	 *
	 * @param {StylesheetDetails} data
	 * @return {*}  {Promise<DataSourceResponse<any>>}
	 * @memberof UmbStylesheetServerDataSource
	 */
	insert(data: StylesheetDetails): Promise<DataSourceResponse<any>> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.postStylesheet({ requestBody: data }));
	}
	/**
	 * Updates an existing Stylesheet
	 *
	 * @param {string} path
	 * @param {StylesheetDetails} data
	 * @return {*}  {Promise<DataSourceResponse<StylesheetDetails>>}
	 * @memberof UmbStylesheetServerDataSource
	 */
	update(path: string, data: StylesheetDetails): Promise<DataSourceResponse<StylesheetDetails>> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.putStylesheet({ requestBody: data }));
	}
	/**
	 * Deletes a Stylesheet.
	 *
	 * @param {string} path
	 * @return {*}  {Promise<DataSourceResponse>}
	 * @memberof UmbStylesheetServerDataSource
	 */
	delete(path: string): Promise<DataSourceResponse> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.deleteStylesheet({ path }));
	}
	/**
	 * Get's the rich text rules for a stylesheet
	 *
	 * @param {string} path
	 * @return {*}  {(Promise<DataSourceResponse<RichTextStylesheetRulesResponseModel | ExtractRichTextStylesheetRulesResponseModel>>)}
	 * @memberof UmbStylesheetServerDataSource
	 */
	getStylesheetRichTextRules(
		path: string
	): Promise<DataSourceResponse<RichTextStylesheetRulesResponseModel | ExtractRichTextStylesheetRulesResponseModel>> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.getStylesheetRichTextRules({ path }));
	}
	/**
	 * Extracts the rich text rules from a stylesheet string. In simple words: takes a stylesheet string and returns a array of rules.
	 *
	 * @param {ExtractRichTextStylesheetRulesRequestModel} data
	 * @return {*}  {Promise<DataSourceResponse<ExtractRichTextStylesheetRulesResponseModel>>}
	 * @memberof UmbStylesheetServerDataSource
	 */
	postStylesheetRichTextExtractRules(
		data: ExtractRichTextStylesheetRulesRequestModel
	): Promise<DataSourceResponse<ExtractRichTextStylesheetRulesResponseModel>> {
		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.postStylesheetRichTextExtractRules({ requestBody: data })
		);
	}
	/**
	 * Interpolates the rich text rules from a stylesheet string. In simple words: takes a array of rules and existing content. Returns new content with the rules applied.
	 *
	 * @param {InterpolateRichTextStylesheetRequestModel} data
	 * @return {*}  {Promise<DataSourceResponse<InterpolateRichTextStylesheetResponseModel>>}
	 * @memberof UmbStylesheetServerDataSource
	 */
	postStylesheetRichTextInterpolateRules(
		data: InterpolateRichTextStylesheetRequestModel
	): Promise<DataSourceResponse<InterpolateRichTextStylesheetResponseModel>> {
		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.postStylesheetRichTextInterpolateRules({ requestBody: data })
		);
	}
}
