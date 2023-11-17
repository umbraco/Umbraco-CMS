import { StylesheetDetails } from '../index.js';
import { UmbStylesheetTreeRepository } from '../tree/index.js';
import { UmbStylesheetServerDataSource } from './sources/stylesheet.server.data.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
	UmbDetailRepository,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateStylesheetRequestModel,
	CreateTextFileViewModelBaseModel,
	ExtractRichTextStylesheetRulesRequestModel,
	ExtractRichTextStylesheetRulesResponseModel,
	InterpolateRichTextStylesheetRequestModel,
	InterpolateRichTextStylesheetResponseModel,
	PagedStylesheetOverviewResponseModel,
	RichTextStylesheetRulesResponseModel,
	TextFileResponseModelBaseModel,
	UpdateStylesheetRequestModel,
	UpdateTextFileViewModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbStylesheetRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<CreateStylesheetRequestModel, string, UpdateStylesheetRequestModel, StylesheetDetails>,
		UmbApi
{
	#dataSource;

	// TODO: temp solution until it is automated
	#treeRepository = new UmbStylesheetTreeRepository(this);

	constructor(host: UmbControllerHostElement) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new UmbStylesheetServerDataSource(this);
	}

	//#region DETAIL:

	createScaffold(
		parentId: string | null,
		preset?: Partial<CreateTextFileViewModelBaseModel> | undefined,
	): Promise<DataSourceResponse<CreateTextFileViewModelBaseModel>> {
		throw new Error('Method not implemented.');
	}

	async requestById(id: string): Promise<DataSourceResponse<TextFileResponseModelBaseModel | undefined>> {
		if (!id) throw new Error('id is missing');
		return this.#dataSource.read(id);
	}

	byId(id: string): Promise<Observable<TextFileResponseModelBaseModel | undefined>> {
		throw new Error('Method not implemented.');
	}

	async create(data: CreateTextFileViewModelBaseModel): Promise<DataSourceResponse<string>> {
		const promise = this.#dataSource.create(data);
		await promise;
		this.#treeRepository.requestTreeItemsOf(data.parentPath ? data.parentPath : null);
		return promise;
	}

	save(id: string, data: UpdateTextFileViewModelBaseModel): Promise<UmbDataSourceErrorResponse> {
		return this.#dataSource.update(id, data);
	}

	delete(id: string): Promise<UmbDataSourceErrorResponse> {
		const promise = this.#dataSource.delete(id);
		const parentPath = id.substring(0, id.lastIndexOf('/'));
		this.#treeRepository.requestTreeItemsOf(parentPath ? parentPath : null);
		return promise;
	}

	async getAll(skip?: number, take?: number): Promise<DataSourceResponse<PagedStylesheetOverviewResponseModel>> {
		return this.#dataSource.getAll(skip, take);
	}

	getStylesheetRules(
		path: string,
	): Promise<DataSourceResponse<RichTextStylesheetRulesResponseModel | ExtractRichTextStylesheetRulesResponseModel>> {
		return this.#dataSource.getStylesheetRichTextRules(path);
	}
	/**
	 * Existing content + array of rules => new content string
	 *
	 * @param {InterpolateRichTextStylesheetRequestModel} data
	 * @return {*}  {Promise<DataSourceResponse<InterpolateRichTextStylesheetResponseModel>>}
	 * @memberof UmbStylesheetRepository
	 */
	interpolateStylesheetRules(
		data: InterpolateRichTextStylesheetRequestModel,
	): Promise<DataSourceResponse<InterpolateRichTextStylesheetResponseModel>> {
		return this.#dataSource.postStylesheetRichTextInterpolateRules(data);
	}
	/**
	 * content string => array of rules
	 *
	 * @param {ExtractRichTextStylesheetRulesRequestModel} data
	 * @return {*}  {Promise<DataSourceResponse<ExtractRichTextStylesheetRulesResponseModel>>}
	 * @memberof UmbStylesheetRepository
	 */
	extractStylesheetRules(
		data: ExtractRichTextStylesheetRulesRequestModel,
	): Promise<DataSourceResponse<ExtractRichTextStylesheetRulesResponseModel>> {
		return this.#dataSource.postStylesheetRichTextExtractRules(data);
	}

	//#endregion
}
