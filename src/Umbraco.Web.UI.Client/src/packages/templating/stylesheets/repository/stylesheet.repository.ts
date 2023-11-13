import { StylesheetDetails } from '../index.js';
import { UmbStylesheetServerDataSource } from './sources/stylesheet.server.data.js';
import {
	StylesheetGetFolderResponse,
	UmbStylesheetFolderServerDataSource,
} from './sources/stylesheet.folder.server.data.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
	UmbDetailRepository,
	UmbFolderRepository,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateFolderRequestModel,
	CreateStylesheetRequestModel,
	CreateTextFileViewModelBaseModel,
	ExtractRichTextStylesheetRulesRequestModel,
	ExtractRichTextStylesheetRulesResponseModel,
	FolderModelBaseModel,
	FolderResponseModel,
	InterpolateRichTextStylesheetRequestModel,
	InterpolateRichTextStylesheetResponseModel,
	PagedStylesheetOverviewResponseModel,
	ProblemDetails,
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
		UmbFolderRepository,
		UmbApi
{
	#dataSource;
	#folderDataSource;

	constructor(host: UmbControllerHostElement) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new UmbStylesheetServerDataSource(this);
		this.#folderDataSource = new UmbStylesheetFolderServerDataSource(this);
	}

	//#region FOLDER:

	createFolderScaffold(
		parentId: string | null,
	): Promise<{ data?: FolderResponseModel | undefined; error?: ProblemDetails | undefined }> {
		const data: FolderResponseModel = {
			name: '',
			parentId,
		};
		return Promise.resolve({ data, error: undefined });
	}

	async createFolder(
		folderRequest: CreateFolderRequestModel,
	): Promise<{ data?: string | undefined; error?: ProblemDetails | undefined }> {
		const req = {
			parentPath: folderRequest.parentId,
			name: folderRequest.name,
		};
		const promise = this.#folderDataSource.insert(req);
		await promise;
		//this.requestTreeItemsOf(folderRequest.parentId ? folderRequest.parentId : null);
		return promise;
	}

	async requestFolder(
		unique: string,
	): Promise<{ data?: StylesheetGetFolderResponse | undefined; error?: ProblemDetails | undefined }> {
		return this.#folderDataSource.get(unique);
	}

	updateFolder(
		unique: string,
		folder: FolderModelBaseModel,
	): Promise<{ data?: FolderModelBaseModel | undefined; error?: ProblemDetails | undefined }> {
		throw new Error('Method not implemented.');
	}

	async deleteFolder(path: string): Promise<{ error?: ProblemDetails | undefined }> {
		const { data } = await this.requestFolder(path);
		const promise = this.#folderDataSource.delete(path);
		await promise;
		//this.requestTreeItemsOf(data?.parentPath ? data?.parentPath : null);
		return promise;
	}

	//#endregion

	//#region DETAIL:

	createScaffold(
		parentId: string | null,
		preset?: Partial<CreateTextFileViewModelBaseModel> | undefined,
	): Promise<DataSourceResponse<CreateTextFileViewModelBaseModel>> {
		throw new Error('Method not implemented.');
	}

	async requestById(id: string): Promise<DataSourceResponse<TextFileResponseModelBaseModel | undefined>> {
		if (!id) throw new Error('id is missing');
		const { data, error } = await this.#dataSource.get(id);
		return { data, error };
	}

	byId(id: string): Promise<Observable<TextFileResponseModelBaseModel | undefined>> {
		throw new Error('Method not implemented.');
	}

	async create(data: CreateTextFileViewModelBaseModel): Promise<DataSourceResponse<string>> {
		const promise = this.#dataSource.insert(data);
		await promise;
		//this.requestTreeItemsOf(data.parentPath ? data.parentPath : null);
		return promise;
	}

	save(id: string, data: UpdateTextFileViewModelBaseModel): Promise<UmbDataSourceErrorResponse> {
		return this.#dataSource.update(id, data);
	}

	delete(id: string): Promise<UmbDataSourceErrorResponse> {
		const promise = this.#dataSource.delete(id);
		const parentPath = id.substring(0, id.lastIndexOf('/'));
		//this.requestTreeItemsOf(parentPath ? parentPath : null);
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
