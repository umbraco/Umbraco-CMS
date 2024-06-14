import type { CancelablePromise } from './core/CancelablePromise';
import { OpenAPI } from './core/OpenAPI';
import { request as __request } from './core/request';
import type { CultureData, DataTypeData, DictionaryData, DocumentBlueprintData, DocumentTypeData, DocumentVersionData, DocumentData, DynamicRootData, HealthCheckData, HelpData, ImagingData, ImportData, IndexerData, InstallData, LanguageData, LogViewerData, ManifestData, MediaTypeData, MediaData, MemberGroupData, MemberTypeData, MemberData, ModelsBuilderData, ObjectTypesData, OembedData, PackageData, PartialViewData, PreviewData, ProfilingData, PropertyTypeData, PublishedCacheData, RedirectManagementData, RelationTypeData, RelationData, ScriptData, SearcherData, SecurityData, SegmentData, ServerData, StaticFileData, StylesheetData, TagData, TelemetryData, TemplateData, TemporaryFileData, UpgradeData, UserDataData, UserGroupData, UserData, WebhookData } from './models';

export class CultureService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getCulture(data: CultureData['payloads']['GetCulture'] = {}): CancelablePromise<CultureData['responses']['GetCulture']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/culture',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class DataTypeService {

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDataType(data: DataTypeData['payloads']['PostDataType'] = {}): CancelablePromise<DataTypeData['responses']['PostDataType']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/data-type',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDataTypeById(data: DataTypeData['payloads']['GetDataTypeById']): CancelablePromise<DataTypeData['responses']['GetDataTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/data-type/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDataTypeById(data: DataTypeData['payloads']['DeleteDataTypeById']): CancelablePromise<DataTypeData['responses']['DeleteDataTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/data-type/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDataTypeById(data: DataTypeData['payloads']['PutDataTypeById']): CancelablePromise<DataTypeData['responses']['PutDataTypeById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/data-type/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDataTypeByIdCopy(data: DataTypeData['payloads']['PostDataTypeByIdCopy']): CancelablePromise<DataTypeData['responses']['PostDataTypeByIdCopy']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/data-type/{id}/copy',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns boolean OK
	 * @throws ApiError
	 */
	public static getDataTypeByIdIsUsed(data: DataTypeData['payloads']['GetDataTypeByIdIsUsed']): CancelablePromise<DataTypeData['responses']['GetDataTypeByIdIsUsed']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/data-type/{id}/is-used',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDataTypeByIdMove(data: DataTypeData['payloads']['PutDataTypeByIdMove']): CancelablePromise<DataTypeData['responses']['PutDataTypeByIdMove']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/data-type/{id}/move',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDataTypeByIdReferences(data: DataTypeData['payloads']['GetDataTypeByIdReferences']): CancelablePromise<DataTypeData['responses']['GetDataTypeByIdReferences']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/data-type/{id}/references',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDataTypeConfiguration(): CancelablePromise<DataTypeData['responses']['GetDataTypeConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/data-type/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDataTypeFolder(data: DataTypeData['payloads']['PostDataTypeFolder'] = {}): CancelablePromise<DataTypeData['responses']['PostDataTypeFolder']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/data-type/folder',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDataTypeFolderById(data: DataTypeData['payloads']['GetDataTypeFolderById']): CancelablePromise<DataTypeData['responses']['GetDataTypeFolderById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/data-type/folder/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDataTypeFolderById(data: DataTypeData['payloads']['DeleteDataTypeFolderById']): CancelablePromise<DataTypeData['responses']['DeleteDataTypeFolderById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/data-type/folder/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDataTypeFolderById(data: DataTypeData['payloads']['PutDataTypeFolderById']): CancelablePromise<DataTypeData['responses']['PutDataTypeFolderById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/data-type/folder/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getFilterDataType(data: DataTypeData['payloads']['GetFilterDataType'] = {}): CancelablePromise<DataTypeData['responses']['GetFilterDataType']> {
		const {
                    
                    skip,
take,
name,
editorUiAlias,
editorAlias
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/filter/data-type',
			query: {
				skip, take, name, editorUiAlias, editorAlias
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemDataType(data: DataTypeData['payloads']['GetItemDataType'] = {}): CancelablePromise<DataTypeData['responses']['GetItemDataType']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/data-type',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemDataTypeSearch(data: DataTypeData['payloads']['GetItemDataTypeSearch'] = {}): CancelablePromise<DataTypeData['responses']['GetItemDataTypeSearch']> {
		const {
                    
                    query,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/data-type/search',
			query: {
				query, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDataTypeAncestors(data: DataTypeData['payloads']['GetTreeDataTypeAncestors'] = {}): CancelablePromise<DataTypeData['responses']['GetTreeDataTypeAncestors']> {
		const {
                    
                    descendantId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/data-type/ancestors',
			query: {
				descendantId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDataTypeChildren(data: DataTypeData['payloads']['GetTreeDataTypeChildren'] = {}): CancelablePromise<DataTypeData['responses']['GetTreeDataTypeChildren']> {
		const {
                    
                    parentId,
skip,
take,
foldersOnly
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/data-type/children',
			query: {
				parentId, skip, take, foldersOnly
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDataTypeRoot(data: DataTypeData['payloads']['GetTreeDataTypeRoot'] = {}): CancelablePromise<DataTypeData['responses']['GetTreeDataTypeRoot']> {
		const {
                    
                    skip,
take,
foldersOnly
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/data-type/root',
			query: {
				skip, take, foldersOnly
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class DictionaryService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDictionary(data: DictionaryData['payloads']['GetDictionary'] = {}): CancelablePromise<DictionaryData['responses']['GetDictionary']> {
		const {
                    
                    filter,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/dictionary',
			query: {
				filter, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDictionary(data: DictionaryData['payloads']['PostDictionary'] = {}): CancelablePromise<DictionaryData['responses']['PostDictionary']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/dictionary',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
				409: `Conflict`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDictionaryById(data: DictionaryData['payloads']['GetDictionaryById']): CancelablePromise<DictionaryData['responses']['GetDictionaryById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/dictionary/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDictionaryById(data: DictionaryData['payloads']['DeleteDictionaryById']): CancelablePromise<DictionaryData['responses']['DeleteDictionaryById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/dictionary/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDictionaryById(data: DictionaryData['payloads']['PutDictionaryById']): CancelablePromise<DictionaryData['responses']['PutDictionaryById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/dictionary/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDictionaryByIdExport(data: DictionaryData['payloads']['GetDictionaryByIdExport']): CancelablePromise<DictionaryData['responses']['GetDictionaryByIdExport']> {
		const {
                    
                    id,
includeChildren
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/dictionary/{id}/export',
			path: {
				id
			},
			query: {
				includeChildren
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDictionaryByIdMove(data: DictionaryData['payloads']['PutDictionaryByIdMove']): CancelablePromise<DictionaryData['responses']['PutDictionaryByIdMove']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/dictionary/{id}/move',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDictionaryImport(data: DictionaryData['payloads']['PostDictionaryImport'] = {}): CancelablePromise<DictionaryData['responses']['PostDictionaryImport']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/dictionary/import',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemDictionary(data: DictionaryData['payloads']['GetItemDictionary'] = {}): CancelablePromise<DictionaryData['responses']['GetItemDictionary']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/dictionary',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDictionaryAncestors(data: DictionaryData['payloads']['GetTreeDictionaryAncestors'] = {}): CancelablePromise<DictionaryData['responses']['GetTreeDictionaryAncestors']> {
		const {
                    
                    descendantId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/dictionary/ancestors',
			query: {
				descendantId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDictionaryChildren(data: DictionaryData['payloads']['GetTreeDictionaryChildren'] = {}): CancelablePromise<DictionaryData['responses']['GetTreeDictionaryChildren']> {
		const {
                    
                    parentId,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/dictionary/children',
			query: {
				parentId, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDictionaryRoot(data: DictionaryData['payloads']['GetTreeDictionaryRoot'] = {}): CancelablePromise<DictionaryData['responses']['GetTreeDictionaryRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/dictionary/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class DocumentBlueprintService {

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentBlueprint(data: DocumentBlueprintData['payloads']['PostDocumentBlueprint'] = {}): CancelablePromise<DocumentBlueprintData['responses']['PostDocumentBlueprint']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-blueprint',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentBlueprintById(data: DocumentBlueprintData['payloads']['GetDocumentBlueprintById']): CancelablePromise<DocumentBlueprintData['responses']['GetDocumentBlueprintById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-blueprint/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDocumentBlueprintById(data: DocumentBlueprintData['payloads']['DeleteDocumentBlueprintById']): CancelablePromise<DocumentBlueprintData['responses']['DeleteDocumentBlueprintById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/document-blueprint/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentBlueprintById(data: DocumentBlueprintData['payloads']['PutDocumentBlueprintById']): CancelablePromise<DocumentBlueprintData['responses']['PutDocumentBlueprintById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document-blueprint/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentBlueprintByIdMove(data: DocumentBlueprintData['payloads']['PutDocumentBlueprintByIdMove']): CancelablePromise<DocumentBlueprintData['responses']['PutDocumentBlueprintByIdMove']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document-blueprint/{id}/move',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentBlueprintFolder(data: DocumentBlueprintData['payloads']['PostDocumentBlueprintFolder'] = {}): CancelablePromise<DocumentBlueprintData['responses']['PostDocumentBlueprintFolder']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-blueprint/folder',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentBlueprintFolderById(data: DocumentBlueprintData['payloads']['GetDocumentBlueprintFolderById']): CancelablePromise<DocumentBlueprintData['responses']['GetDocumentBlueprintFolderById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-blueprint/folder/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDocumentBlueprintFolderById(data: DocumentBlueprintData['payloads']['DeleteDocumentBlueprintFolderById']): CancelablePromise<DocumentBlueprintData['responses']['DeleteDocumentBlueprintFolderById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/document-blueprint/folder/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentBlueprintFolderById(data: DocumentBlueprintData['payloads']['PutDocumentBlueprintFolderById']): CancelablePromise<DocumentBlueprintData['responses']['PutDocumentBlueprintFolderById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document-blueprint/folder/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentBlueprintFromDocument(data: DocumentBlueprintData['payloads']['PostDocumentBlueprintFromDocument'] = {}): CancelablePromise<DocumentBlueprintData['responses']['PostDocumentBlueprintFromDocument']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-blueprint/from-document',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemDocumentBlueprint(data: DocumentBlueprintData['payloads']['GetItemDocumentBlueprint'] = {}): CancelablePromise<DocumentBlueprintData['responses']['GetItemDocumentBlueprint']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/document-blueprint',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentBlueprintAncestors(data: DocumentBlueprintData['payloads']['GetTreeDocumentBlueprintAncestors'] = {}): CancelablePromise<DocumentBlueprintData['responses']['GetTreeDocumentBlueprintAncestors']> {
		const {
                    
                    descendantId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document-blueprint/ancestors',
			query: {
				descendantId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentBlueprintChildren(data: DocumentBlueprintData['payloads']['GetTreeDocumentBlueprintChildren'] = {}): CancelablePromise<DocumentBlueprintData['responses']['GetTreeDocumentBlueprintChildren']> {
		const {
                    
                    parentId,
skip,
take,
foldersOnly
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document-blueprint/children',
			query: {
				parentId, skip, take, foldersOnly
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentBlueprintRoot(data: DocumentBlueprintData['payloads']['GetTreeDocumentBlueprintRoot'] = {}): CancelablePromise<DocumentBlueprintData['responses']['GetTreeDocumentBlueprintRoot']> {
		const {
                    
                    skip,
take,
foldersOnly
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document-blueprint/root',
			query: {
				skip, take, foldersOnly
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class DocumentTypeService {

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentType(data: DocumentTypeData['payloads']['PostDocumentType'] = {}): CancelablePromise<DocumentTypeData['responses']['PostDocumentType']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-type',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentTypeById(data: DocumentTypeData['payloads']['GetDocumentTypeById']): CancelablePromise<DocumentTypeData['responses']['GetDocumentTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-type/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDocumentTypeById(data: DocumentTypeData['payloads']['DeleteDocumentTypeById']): CancelablePromise<DocumentTypeData['responses']['DeleteDocumentTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/document-type/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentTypeById(data: DocumentTypeData['payloads']['PutDocumentTypeById']): CancelablePromise<DocumentTypeData['responses']['PutDocumentTypeById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document-type/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentTypeByIdAllowedChildren(data: DocumentTypeData['payloads']['GetDocumentTypeByIdAllowedChildren']): CancelablePromise<DocumentTypeData['responses']['GetDocumentTypeByIdAllowedChildren']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-type/{id}/allowed-children',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentTypeByIdBlueprint(data: DocumentTypeData['payloads']['GetDocumentTypeByIdBlueprint']): CancelablePromise<DocumentTypeData['responses']['GetDocumentTypeByIdBlueprint']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-type/{id}/blueprint',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentTypeByIdCompositionReferences(data: DocumentTypeData['payloads']['GetDocumentTypeByIdCompositionReferences']): CancelablePromise<DocumentTypeData['responses']['GetDocumentTypeByIdCompositionReferences']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-type/{id}/composition-references',
			path: {
				id
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentTypeByIdCopy(data: DocumentTypeData['payloads']['PostDocumentTypeByIdCopy']): CancelablePromise<DocumentTypeData['responses']['PostDocumentTypeByIdCopy']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-type/{id}/copy',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentTypeByIdExport(data: DocumentTypeData['payloads']['GetDocumentTypeByIdExport']): CancelablePromise<DocumentTypeData['responses']['GetDocumentTypeByIdExport']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-type/{id}/export',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentTypeByIdImport(data: DocumentTypeData['payloads']['PutDocumentTypeByIdImport']): CancelablePromise<DocumentTypeData['responses']['PutDocumentTypeByIdImport']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document-type/{id}/import',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentTypeByIdMove(data: DocumentTypeData['payloads']['PutDocumentTypeByIdMove']): CancelablePromise<DocumentTypeData['responses']['PutDocumentTypeByIdMove']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document-type/{id}/move',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentTypeAllowedAtRoot(data: DocumentTypeData['payloads']['GetDocumentTypeAllowedAtRoot'] = {}): CancelablePromise<DocumentTypeData['responses']['GetDocumentTypeAllowedAtRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-type/allowed-at-root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postDocumentTypeAvailableCompositions(data: DocumentTypeData['payloads']['PostDocumentTypeAvailableCompositions'] = {}): CancelablePromise<DocumentTypeData['responses']['PostDocumentTypeAvailableCompositions']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-type/available-compositions',
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentTypeConfiguration(): CancelablePromise<DocumentTypeData['responses']['GetDocumentTypeConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-type/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentTypeFolder(data: DocumentTypeData['payloads']['PostDocumentTypeFolder'] = {}): CancelablePromise<DocumentTypeData['responses']['PostDocumentTypeFolder']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-type/folder',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentTypeFolderById(data: DocumentTypeData['payloads']['GetDocumentTypeFolderById']): CancelablePromise<DocumentTypeData['responses']['GetDocumentTypeFolderById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-type/folder/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDocumentTypeFolderById(data: DocumentTypeData['payloads']['DeleteDocumentTypeFolderById']): CancelablePromise<DocumentTypeData['responses']['DeleteDocumentTypeFolderById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/document-type/folder/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentTypeFolderById(data: DocumentTypeData['payloads']['PutDocumentTypeFolderById']): CancelablePromise<DocumentTypeData['responses']['PutDocumentTypeFolderById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document-type/folder/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentTypeImport(data: DocumentTypeData['payloads']['PostDocumentTypeImport'] = {}): CancelablePromise<DocumentTypeData['responses']['PostDocumentTypeImport']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-type/import',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemDocumentType(data: DocumentTypeData['payloads']['GetItemDocumentType'] = {}): CancelablePromise<DocumentTypeData['responses']['GetItemDocumentType']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/document-type',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemDocumentTypeSearch(data: DocumentTypeData['payloads']['GetItemDocumentTypeSearch'] = {}): CancelablePromise<DocumentTypeData['responses']['GetItemDocumentTypeSearch']> {
		const {
                    
                    query,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/document-type/search',
			query: {
				query, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentTypeAncestors(data: DocumentTypeData['payloads']['GetTreeDocumentTypeAncestors'] = {}): CancelablePromise<DocumentTypeData['responses']['GetTreeDocumentTypeAncestors']> {
		const {
                    
                    descendantId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document-type/ancestors',
			query: {
				descendantId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentTypeChildren(data: DocumentTypeData['payloads']['GetTreeDocumentTypeChildren'] = {}): CancelablePromise<DocumentTypeData['responses']['GetTreeDocumentTypeChildren']> {
		const {
                    
                    parentId,
skip,
take,
foldersOnly
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document-type/children',
			query: {
				parentId, skip, take, foldersOnly
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentTypeRoot(data: DocumentTypeData['payloads']['GetTreeDocumentTypeRoot'] = {}): CancelablePromise<DocumentTypeData['responses']['GetTreeDocumentTypeRoot']> {
		const {
                    
                    skip,
take,
foldersOnly
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document-type/root',
			query: {
				skip, take, foldersOnly
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class DocumentVersionService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentVersion(data: DocumentVersionData['payloads']['GetDocumentVersion']): CancelablePromise<DocumentVersionData['responses']['GetDocumentVersion']> {
		const {
                    
                    documentId,
culture,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-version',
			query: {
				documentId, culture, skip, take
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentVersionById(data: DocumentVersionData['payloads']['GetDocumentVersionById']): CancelablePromise<DocumentVersionData['responses']['GetDocumentVersionById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document-version/{id}',
			path: {
				id
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentVersionByIdPreventCleanup(data: DocumentVersionData['payloads']['PutDocumentVersionByIdPreventCleanup']): CancelablePromise<DocumentVersionData['responses']['PutDocumentVersionByIdPreventCleanup']> {
		const {
                    
                    id,
preventCleanup
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document-version/{id}/prevent-cleanup',
			path: {
				id
			},
			query: {
				preventCleanup
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postDocumentVersionByIdRollback(data: DocumentVersionData['payloads']['PostDocumentVersionByIdRollback']): CancelablePromise<DocumentVersionData['responses']['PostDocumentVersionByIdRollback']> {
		const {
                    
                    id,
culture
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document-version/{id}/rollback',
			path: {
				id
			},
			query: {
				culture
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

}

export class DocumentService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getCollectionDocumentById(data: DocumentData['payloads']['GetCollectionDocumentById']): CancelablePromise<DocumentData['responses']['GetCollectionDocumentById']> {
		const {
                    
                    id,
dataTypeId,
orderBy,
orderCulture,
orderDirection,
filter,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/collection/document/{id}',
			path: {
				id
			},
			query: {
				dataTypeId, orderBy, orderCulture, orderDirection, filter, skip, take
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocument(data: DocumentData['payloads']['PostDocument'] = {}): CancelablePromise<DocumentData['responses']['PostDocument']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentById(data: DocumentData['payloads']['GetDocumentById']): CancelablePromise<DocumentData['responses']['GetDocumentById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDocumentById(data: DocumentData['payloads']['DeleteDocumentById']): CancelablePromise<DocumentData['responses']['DeleteDocumentById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/document/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentById(data: DocumentData['payloads']['PutDocumentById']): CancelablePromise<DocumentData['responses']['PutDocumentById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentByIdAuditLog(data: DocumentData['payloads']['GetDocumentByIdAuditLog']): CancelablePromise<DocumentData['responses']['GetDocumentByIdAuditLog']> {
		const {
                    
                    id,
orderDirection,
sinceDate,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/{id}/audit-log',
			path: {
				id
			},
			query: {
				orderDirection, sinceDate, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentByIdCopy(data: DocumentData['payloads']['PostDocumentByIdCopy']): CancelablePromise<DocumentData['responses']['PostDocumentByIdCopy']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document/{id}/copy',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentByIdDomains(data: DocumentData['payloads']['GetDocumentByIdDomains']): CancelablePromise<DocumentData['responses']['GetDocumentByIdDomains']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/{id}/domains',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdDomains(data: DocumentData['payloads']['PutDocumentByIdDomains']): CancelablePromise<DocumentData['responses']['PutDocumentByIdDomains']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/domains',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
				409: `Conflict`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdMove(data: DocumentData['payloads']['PutDocumentByIdMove']): CancelablePromise<DocumentData['responses']['PutDocumentByIdMove']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/move',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdMoveToRecycleBin(data: DocumentData['payloads']['PutDocumentByIdMoveToRecycleBin']): CancelablePromise<DocumentData['responses']['PutDocumentByIdMoveToRecycleBin']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/move-to-recycle-bin',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentByIdNotifications(data: DocumentData['payloads']['GetDocumentByIdNotifications']): CancelablePromise<DocumentData['responses']['GetDocumentByIdNotifications']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/{id}/notifications',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdNotifications(data: DocumentData['payloads']['PutDocumentByIdNotifications']): CancelablePromise<DocumentData['responses']['PutDocumentByIdNotifications']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/notifications',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postDocumentByIdPublicAccess(data: DocumentData['payloads']['PostDocumentByIdPublicAccess']): CancelablePromise<DocumentData['responses']['PostDocumentByIdPublicAccess']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document/{id}/public-access',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteDocumentByIdPublicAccess(data: DocumentData['payloads']['DeleteDocumentByIdPublicAccess']): CancelablePromise<DocumentData['responses']['DeleteDocumentByIdPublicAccess']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/document/{id}/public-access',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentByIdPublicAccess(data: DocumentData['payloads']['GetDocumentByIdPublicAccess']): CancelablePromise<DocumentData['responses']['GetDocumentByIdPublicAccess']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/{id}/public-access',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdPublicAccess(data: DocumentData['payloads']['PutDocumentByIdPublicAccess']): CancelablePromise<DocumentData['responses']['PutDocumentByIdPublicAccess']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/public-access',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdPublish(data: DocumentData['payloads']['PutDocumentByIdPublish']): CancelablePromise<DocumentData['responses']['PutDocumentByIdPublish']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/publish',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdPublishWithDescendants(data: DocumentData['payloads']['PutDocumentByIdPublishWithDescendants']): CancelablePromise<DocumentData['responses']['PutDocumentByIdPublishWithDescendants']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/publish-with-descendants',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentByIdReferencedBy(data: DocumentData['payloads']['GetDocumentByIdReferencedBy']): CancelablePromise<DocumentData['responses']['GetDocumentByIdReferencedBy']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/{id}/referenced-by',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentByIdReferencedDescendants(data: DocumentData['payloads']['GetDocumentByIdReferencedDescendants']): CancelablePromise<DocumentData['responses']['GetDocumentByIdReferencedDescendants']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/{id}/referenced-descendants',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdUnpublish(data: DocumentData['payloads']['PutDocumentByIdUnpublish']): CancelablePromise<DocumentData['responses']['PutDocumentByIdUnpublish']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/unpublish',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentByIdValidate(data: DocumentData['payloads']['PutDocumentByIdValidate']): CancelablePromise<DocumentData['responses']['PutDocumentByIdValidate']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/{id}/validate',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentAreReferenced(data: DocumentData['payloads']['GetDocumentAreReferenced'] = {}): CancelablePromise<DocumentData['responses']['GetDocumentAreReferenced']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/are-referenced',
			query: {
				id, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentConfiguration(): CancelablePromise<DocumentData['responses']['GetDocumentConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putDocumentSort(data: DocumentData['payloads']['PutDocumentSort'] = {}): CancelablePromise<DocumentData['responses']['PutDocumentSort']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/document/sort',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getDocumentUrls(data: DocumentData['payloads']['GetDocumentUrls'] = {}): CancelablePromise<DocumentData['responses']['GetDocumentUrls']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/document/urls',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postDocumentValidate(data: DocumentData['payloads']['PostDocumentValidate'] = {}): CancelablePromise<DocumentData['responses']['PostDocumentValidate']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/document/validate',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemDocument(data: DocumentData['payloads']['GetItemDocument'] = {}): CancelablePromise<DocumentData['responses']['GetItemDocument']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/document',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemDocumentSearch(data: DocumentData['payloads']['GetItemDocumentSearch'] = {}): CancelablePromise<DocumentData['responses']['GetItemDocumentSearch']> {
		const {
                    
                    query,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/document/search',
			query: {
				query, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteRecycleBinDocument(): CancelablePromise<DocumentData['responses']['DeleteRecycleBinDocument']> {
		
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/recycle-bin/document',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteRecycleBinDocumentById(data: DocumentData['payloads']['DeleteRecycleBinDocumentById']): CancelablePromise<DocumentData['responses']['DeleteRecycleBinDocumentById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/recycle-bin/document/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRecycleBinDocumentByIdOriginalParent(data: DocumentData['payloads']['GetRecycleBinDocumentByIdOriginalParent']): CancelablePromise<DocumentData['responses']['GetRecycleBinDocumentByIdOriginalParent']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/recycle-bin/document/{id}/original-parent',
			path: {
				id
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putRecycleBinDocumentByIdRestore(data: DocumentData['payloads']['PutRecycleBinDocumentByIdRestore']): CancelablePromise<DocumentData['responses']['PutRecycleBinDocumentByIdRestore']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/recycle-bin/document/{id}/restore',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRecycleBinDocumentChildren(data: DocumentData['payloads']['GetRecycleBinDocumentChildren'] = {}): CancelablePromise<DocumentData['responses']['GetRecycleBinDocumentChildren']> {
		const {
                    
                    parentId,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/recycle-bin/document/children',
			query: {
				parentId, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRecycleBinDocumentRoot(data: DocumentData['payloads']['GetRecycleBinDocumentRoot'] = {}): CancelablePromise<DocumentData['responses']['GetRecycleBinDocumentRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/recycle-bin/document/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentAncestors(data: DocumentData['payloads']['GetTreeDocumentAncestors'] = {}): CancelablePromise<DocumentData['responses']['GetTreeDocumentAncestors']> {
		const {
                    
                    descendantId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document/ancestors',
			query: {
				descendantId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentChildren(data: DocumentData['payloads']['GetTreeDocumentChildren'] = {}): CancelablePromise<DocumentData['responses']['GetTreeDocumentChildren']> {
		const {
                    
                    parentId,
skip,
take,
dataTypeId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document/children',
			query: {
				parentId, skip, take, dataTypeId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeDocumentRoot(data: DocumentData['payloads']['GetTreeDocumentRoot'] = {}): CancelablePromise<DocumentData['responses']['GetTreeDocumentRoot']> {
		const {
                    
                    skip,
take,
dataTypeId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/document/root',
			query: {
				skip, take, dataTypeId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class DynamicRootService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postDynamicRootQuery(data: DynamicRootData['payloads']['PostDynamicRootQuery'] = {}): CancelablePromise<DynamicRootData['responses']['PostDynamicRootQuery']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/dynamic-root/query',
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static getDynamicRootSteps(): CancelablePromise<DynamicRootData['responses']['GetDynamicRootSteps']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/dynamic-root/steps',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class HealthCheckService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getHealthCheckGroup(data: HealthCheckData['payloads']['GetHealthCheckGroup'] = {}): CancelablePromise<HealthCheckData['responses']['GetHealthCheckGroup']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/health-check-group',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getHealthCheckGroupByName(data: HealthCheckData['payloads']['GetHealthCheckGroupByName']): CancelablePromise<HealthCheckData['responses']['GetHealthCheckGroupByName']> {
		const {
                    
                    name
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/health-check-group/{name}',
			path: {
				name
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postHealthCheckGroupByNameCheck(data: HealthCheckData['payloads']['PostHealthCheckGroupByNameCheck']): CancelablePromise<HealthCheckData['responses']['PostHealthCheckGroupByNameCheck']> {
		const {
                    
                    name
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/health-check-group/{name}/check',
			path: {
				name
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postHealthCheckExecuteAction(data: HealthCheckData['payloads']['PostHealthCheckExecuteAction'] = {}): CancelablePromise<HealthCheckData['responses']['PostHealthCheckExecuteAction']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/health-check/execute-action',
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class HelpService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getHelp(data: HelpData['payloads']['GetHelp'] = {}): CancelablePromise<HelpData['responses']['GetHelp']> {
		const {
                    
                    section,
tree,
skip,
take,
baseUrl
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/help',
			query: {
				section, tree, skip, take, baseUrl
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class ImagingService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getImagingResizeUrls(data: ImagingData['payloads']['GetImagingResizeUrls'] = {}): CancelablePromise<ImagingData['responses']['GetImagingResizeUrls']> {
		const {
                    
                    id,
height,
width,
mode
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/imaging/resize/urls',
			query: {
				id, height, width, mode
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class ImportService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getImportAnalyze(data: ImportData['payloads']['GetImportAnalyze'] = {}): CancelablePromise<ImportData['responses']['GetImportAnalyze']> {
		const {
                    
                    temporaryFileId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/import/analyze',
			query: {
				temporaryFileId
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

}

export class IndexerService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getIndexer(data: IndexerData['payloads']['GetIndexer'] = {}): CancelablePromise<IndexerData['responses']['GetIndexer']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/indexer',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getIndexerByIndexName(data: IndexerData['payloads']['GetIndexerByIndexName']): CancelablePromise<IndexerData['responses']['GetIndexerByIndexName']> {
		const {
                    
                    indexName
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/indexer/{indexName}',
			path: {
				indexName
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postIndexerByIndexNameRebuild(data: IndexerData['payloads']['PostIndexerByIndexNameRebuild']): CancelablePromise<IndexerData['responses']['PostIndexerByIndexNameRebuild']> {
		const {
                    
                    indexName
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/indexer/{indexName}/rebuild',
			path: {
				indexName
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
				409: `Conflict`,
			},
		});
	}

}

export class InstallService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getInstallSettings(): CancelablePromise<InstallData['responses']['GetInstallSettings']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/install/settings',
			errors: {
				428: `Precondition Required`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postInstallSetup(data: InstallData['payloads']['PostInstallSetup'] = {}): CancelablePromise<InstallData['responses']['PostInstallSetup']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/install/setup',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				428: `Precondition Required`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postInstallValidateDatabase(data: InstallData['payloads']['PostInstallValidateDatabase'] = {}): CancelablePromise<InstallData['responses']['PostInstallValidateDatabase']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/install/validate-database',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
			},
		});
	}

}

export class LanguageService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemLanguage(data: LanguageData['payloads']['GetItemLanguage'] = {}): CancelablePromise<LanguageData['responses']['GetItemLanguage']> {
		const {
                    
                    isoCode
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/language',
			query: {
				isoCode
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemLanguageDefault(): CancelablePromise<LanguageData['responses']['GetItemLanguageDefault']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/language/default',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getLanguage(data: LanguageData['payloads']['GetLanguage'] = {}): CancelablePromise<LanguageData['responses']['GetLanguage']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/language',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postLanguage(data: LanguageData['payloads']['PostLanguage'] = {}): CancelablePromise<LanguageData['responses']['PostLanguage']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/language',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getLanguageByIsoCode(data: LanguageData['payloads']['GetLanguageByIsoCode']): CancelablePromise<LanguageData['responses']['GetLanguageByIsoCode']> {
		const {
                    
                    isoCode
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/language/{isoCode}',
			path: {
				isoCode
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteLanguageByIsoCode(data: LanguageData['payloads']['DeleteLanguageByIsoCode']): CancelablePromise<LanguageData['responses']['DeleteLanguageByIsoCode']> {
		const {
                    
                    isoCode
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/language/{isoCode}',
			path: {
				isoCode
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putLanguageByIsoCode(data: LanguageData['payloads']['PutLanguageByIsoCode']): CancelablePromise<LanguageData['responses']['PutLanguageByIsoCode']> {
		const {
                    
                    isoCode,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/language/{isoCode}',
			path: {
				isoCode
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

}

export class LogViewerService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getLogViewerLevel(data: LogViewerData['payloads']['GetLogViewerLevel'] = {}): CancelablePromise<LogViewerData['responses']['GetLogViewerLevel']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/log-viewer/level',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getLogViewerLevelCount(data: LogViewerData['payloads']['GetLogViewerLevelCount'] = {}): CancelablePromise<LogViewerData['responses']['GetLogViewerLevelCount']> {
		const {
                    
                    startDate,
endDate
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/log-viewer/level-count',
			query: {
				startDate, endDate
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getLogViewerLog(data: LogViewerData['payloads']['GetLogViewerLog'] = {}): CancelablePromise<LogViewerData['responses']['GetLogViewerLog']> {
		const {
                    
                    skip,
take,
orderDirection,
filterExpression,
logLevel,
startDate,
endDate
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/log-viewer/log',
			query: {
				skip, take, orderDirection, filterExpression, logLevel, startDate, endDate
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getLogViewerMessageTemplate(data: LogViewerData['payloads']['GetLogViewerMessageTemplate'] = {}): CancelablePromise<LogViewerData['responses']['GetLogViewerMessageTemplate']> {
		const {
                    
                    skip,
take,
startDate,
endDate
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/log-viewer/message-template',
			query: {
				skip, take, startDate, endDate
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getLogViewerSavedSearch(data: LogViewerData['payloads']['GetLogViewerSavedSearch'] = {}): CancelablePromise<LogViewerData['responses']['GetLogViewerSavedSearch']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/log-viewer/saved-search',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postLogViewerSavedSearch(data: LogViewerData['payloads']['PostLogViewerSavedSearch'] = {}): CancelablePromise<LogViewerData['responses']['PostLogViewerSavedSearch']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/log-viewer/saved-search',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getLogViewerSavedSearchByName(data: LogViewerData['payloads']['GetLogViewerSavedSearchByName']): CancelablePromise<LogViewerData['responses']['GetLogViewerSavedSearchByName']> {
		const {
                    
                    name
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/log-viewer/saved-search/{name}',
			path: {
				name
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteLogViewerSavedSearchByName(data: LogViewerData['payloads']['DeleteLogViewerSavedSearchByName']): CancelablePromise<LogViewerData['responses']['DeleteLogViewerSavedSearchByName']> {
		const {
                    
                    name
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/log-viewer/saved-search/{name}',
			path: {
				name
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns any OK
	 * @throws ApiError
	 */
	public static getLogViewerValidateLogsSize(data: LogViewerData['payloads']['GetLogViewerValidateLogsSize'] = {}): CancelablePromise<LogViewerData['responses']['GetLogViewerValidateLogsSize']> {
		const {
                    
                    startDate,
endDate
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/log-viewer/validate-logs-size',
			query: {
				startDate, endDate
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class ManifestService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getManifestManifest(): CancelablePromise<ManifestData['responses']['GetManifestManifest']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/manifest/manifest',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getManifestManifestPrivate(): CancelablePromise<ManifestData['responses']['GetManifestManifestPrivate']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/manifest/manifest/private',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getManifestManifestPublic(): CancelablePromise<ManifestData['responses']['GetManifestManifestPublic']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/manifest/manifest/public',
		});
	}

}

export class MediaTypeService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMediaType(data: MediaTypeData['payloads']['GetItemMediaType'] = {}): CancelablePromise<MediaTypeData['responses']['GetItemMediaType']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/media-type',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMediaTypeAllowed(data: MediaTypeData['payloads']['GetItemMediaTypeAllowed'] = {}): CancelablePromise<MediaTypeData['responses']['GetItemMediaTypeAllowed']> {
		const {
                    
                    fileExtension,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/media-type/allowed',
			query: {
				fileExtension, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMediaTypeFolders(data: MediaTypeData['payloads']['GetItemMediaTypeFolders'] = {}): CancelablePromise<MediaTypeData['responses']['GetItemMediaTypeFolders']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/media-type/folders',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMediaTypeSearch(data: MediaTypeData['payloads']['GetItemMediaTypeSearch'] = {}): CancelablePromise<MediaTypeData['responses']['GetItemMediaTypeSearch']> {
		const {
                    
                    query,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/media-type/search',
			query: {
				query, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMediaType(data: MediaTypeData['payloads']['PostMediaType'] = {}): CancelablePromise<MediaTypeData['responses']['PostMediaType']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/media-type',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaTypeById(data: MediaTypeData['payloads']['GetMediaTypeById']): CancelablePromise<MediaTypeData['responses']['GetMediaTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media-type/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteMediaTypeById(data: MediaTypeData['payloads']['DeleteMediaTypeById']): CancelablePromise<MediaTypeData['responses']['DeleteMediaTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/media-type/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaTypeById(data: MediaTypeData['payloads']['PutMediaTypeById']): CancelablePromise<MediaTypeData['responses']['PutMediaTypeById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media-type/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaTypeByIdAllowedChildren(data: MediaTypeData['payloads']['GetMediaTypeByIdAllowedChildren']): CancelablePromise<MediaTypeData['responses']['GetMediaTypeByIdAllowedChildren']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media-type/{id}/allowed-children',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaTypeByIdCompositionReferences(data: MediaTypeData['payloads']['GetMediaTypeByIdCompositionReferences']): CancelablePromise<MediaTypeData['responses']['GetMediaTypeByIdCompositionReferences']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media-type/{id}/composition-references',
			path: {
				id
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMediaTypeByIdCopy(data: MediaTypeData['payloads']['PostMediaTypeByIdCopy']): CancelablePromise<MediaTypeData['responses']['PostMediaTypeByIdCopy']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/media-type/{id}/copy',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaTypeByIdExport(data: MediaTypeData['payloads']['GetMediaTypeByIdExport']): CancelablePromise<MediaTypeData['responses']['GetMediaTypeByIdExport']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media-type/{id}/export',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaTypeByIdImport(data: MediaTypeData['payloads']['PutMediaTypeByIdImport']): CancelablePromise<MediaTypeData['responses']['PutMediaTypeByIdImport']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media-type/{id}/import',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaTypeByIdMove(data: MediaTypeData['payloads']['PutMediaTypeByIdMove']): CancelablePromise<MediaTypeData['responses']['PutMediaTypeByIdMove']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media-type/{id}/move',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaTypeAllowedAtRoot(data: MediaTypeData['payloads']['GetMediaTypeAllowedAtRoot'] = {}): CancelablePromise<MediaTypeData['responses']['GetMediaTypeAllowedAtRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media-type/allowed-at-root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postMediaTypeAvailableCompositions(data: MediaTypeData['payloads']['PostMediaTypeAvailableCompositions'] = {}): CancelablePromise<MediaTypeData['responses']['PostMediaTypeAvailableCompositions']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/media-type/available-compositions',
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMediaTypeFolder(data: MediaTypeData['payloads']['PostMediaTypeFolder'] = {}): CancelablePromise<MediaTypeData['responses']['PostMediaTypeFolder']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/media-type/folder',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaTypeFolderById(data: MediaTypeData['payloads']['GetMediaTypeFolderById']): CancelablePromise<MediaTypeData['responses']['GetMediaTypeFolderById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media-type/folder/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteMediaTypeFolderById(data: MediaTypeData['payloads']['DeleteMediaTypeFolderById']): CancelablePromise<MediaTypeData['responses']['DeleteMediaTypeFolderById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/media-type/folder/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaTypeFolderById(data: MediaTypeData['payloads']['PutMediaTypeFolderById']): CancelablePromise<MediaTypeData['responses']['PutMediaTypeFolderById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media-type/folder/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMediaTypeImport(data: MediaTypeData['payloads']['PostMediaTypeImport'] = {}): CancelablePromise<MediaTypeData['responses']['PostMediaTypeImport']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/media-type/import',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeMediaTypeAncestors(data: MediaTypeData['payloads']['GetTreeMediaTypeAncestors'] = {}): CancelablePromise<MediaTypeData['responses']['GetTreeMediaTypeAncestors']> {
		const {
                    
                    descendantId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/media-type/ancestors',
			query: {
				descendantId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeMediaTypeChildren(data: MediaTypeData['payloads']['GetTreeMediaTypeChildren'] = {}): CancelablePromise<MediaTypeData['responses']['GetTreeMediaTypeChildren']> {
		const {
                    
                    parentId,
skip,
take,
foldersOnly
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/media-type/children',
			query: {
				parentId, skip, take, foldersOnly
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeMediaTypeRoot(data: MediaTypeData['payloads']['GetTreeMediaTypeRoot'] = {}): CancelablePromise<MediaTypeData['responses']['GetTreeMediaTypeRoot']> {
		const {
                    
                    skip,
take,
foldersOnly
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/media-type/root',
			query: {
				skip, take, foldersOnly
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class MediaService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getCollectionMedia(data: MediaData['payloads']['GetCollectionMedia'] = {}): CancelablePromise<MediaData['responses']['GetCollectionMedia']> {
		const {
                    
                    id,
dataTypeId,
orderBy,
orderDirection,
filter,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/collection/media',
			query: {
				id, dataTypeId, orderBy, orderDirection, filter, skip, take
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMedia(data: MediaData['payloads']['GetItemMedia'] = {}): CancelablePromise<MediaData['responses']['GetItemMedia']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/media',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMediaSearch(data: MediaData['payloads']['GetItemMediaSearch'] = {}): CancelablePromise<MediaData['responses']['GetItemMediaSearch']> {
		const {
                    
                    query,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/media/search',
			query: {
				query, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMedia(data: MediaData['payloads']['PostMedia'] = {}): CancelablePromise<MediaData['responses']['PostMedia']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/media',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaById(data: MediaData['payloads']['GetMediaById']): CancelablePromise<MediaData['responses']['GetMediaById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteMediaById(data: MediaData['payloads']['DeleteMediaById']): CancelablePromise<MediaData['responses']['DeleteMediaById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/media/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaById(data: MediaData['payloads']['PutMediaById']): CancelablePromise<MediaData['responses']['PutMediaById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaByIdAuditLog(data: MediaData['payloads']['GetMediaByIdAuditLog']): CancelablePromise<MediaData['responses']['GetMediaByIdAuditLog']> {
		const {
                    
                    id,
orderDirection,
sinceDate,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media/{id}/audit-log',
			path: {
				id
			},
			query: {
				orderDirection, sinceDate, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaByIdMove(data: MediaData['payloads']['PutMediaByIdMove']): CancelablePromise<MediaData['responses']['PutMediaByIdMove']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media/{id}/move',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaByIdMoveToRecycleBin(data: MediaData['payloads']['PutMediaByIdMoveToRecycleBin']): CancelablePromise<MediaData['responses']['PutMediaByIdMoveToRecycleBin']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media/{id}/move-to-recycle-bin',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaByIdReferencedBy(data: MediaData['payloads']['GetMediaByIdReferencedBy']): CancelablePromise<MediaData['responses']['GetMediaByIdReferencedBy']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media/{id}/referenced-by',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaByIdReferencedDescendants(data: MediaData['payloads']['GetMediaByIdReferencedDescendants']): CancelablePromise<MediaData['responses']['GetMediaByIdReferencedDescendants']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media/{id}/referenced-descendants',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaByIdValidate(data: MediaData['payloads']['PutMediaByIdValidate']): CancelablePromise<MediaData['responses']['PutMediaByIdValidate']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media/{id}/validate',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaAreReferenced(data: MediaData['payloads']['GetMediaAreReferenced'] = {}): CancelablePromise<MediaData['responses']['GetMediaAreReferenced']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media/are-referenced',
			query: {
				id, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaConfiguration(): CancelablePromise<MediaData['responses']['GetMediaConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMediaSort(data: MediaData['payloads']['PutMediaSort'] = {}): CancelablePromise<MediaData['responses']['PutMediaSort']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/media/sort',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMediaUrls(data: MediaData['payloads']['GetMediaUrls'] = {}): CancelablePromise<MediaData['responses']['GetMediaUrls']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/media/urls',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postMediaValidate(data: MediaData['payloads']['PostMediaValidate'] = {}): CancelablePromise<MediaData['responses']['PostMediaValidate']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/media/validate',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteRecycleBinMedia(): CancelablePromise<MediaData['responses']['DeleteRecycleBinMedia']> {
		
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/recycle-bin/media',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteRecycleBinMediaById(data: MediaData['payloads']['DeleteRecycleBinMediaById']): CancelablePromise<MediaData['responses']['DeleteRecycleBinMediaById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/recycle-bin/media/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRecycleBinMediaByIdOriginalParent(data: MediaData['payloads']['GetRecycleBinMediaByIdOriginalParent']): CancelablePromise<MediaData['responses']['GetRecycleBinMediaByIdOriginalParent']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/recycle-bin/media/{id}/original-parent',
			path: {
				id
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putRecycleBinMediaByIdRestore(data: MediaData['payloads']['PutRecycleBinMediaByIdRestore']): CancelablePromise<MediaData['responses']['PutRecycleBinMediaByIdRestore']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/recycle-bin/media/{id}/restore',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRecycleBinMediaChildren(data: MediaData['payloads']['GetRecycleBinMediaChildren'] = {}): CancelablePromise<MediaData['responses']['GetRecycleBinMediaChildren']> {
		const {
                    
                    parentId,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/recycle-bin/media/children',
			query: {
				parentId, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRecycleBinMediaRoot(data: MediaData['payloads']['GetRecycleBinMediaRoot'] = {}): CancelablePromise<MediaData['responses']['GetRecycleBinMediaRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/recycle-bin/media/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeMediaAncestors(data: MediaData['payloads']['GetTreeMediaAncestors'] = {}): CancelablePromise<MediaData['responses']['GetTreeMediaAncestors']> {
		const {
                    
                    descendantId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/media/ancestors',
			query: {
				descendantId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeMediaChildren(data: MediaData['payloads']['GetTreeMediaChildren'] = {}): CancelablePromise<MediaData['responses']['GetTreeMediaChildren']> {
		const {
                    
                    parentId,
skip,
take,
dataTypeId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/media/children',
			query: {
				parentId, skip, take, dataTypeId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeMediaRoot(data: MediaData['payloads']['GetTreeMediaRoot'] = {}): CancelablePromise<MediaData['responses']['GetTreeMediaRoot']> {
		const {
                    
                    skip,
take,
dataTypeId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/media/root',
			query: {
				skip, take, dataTypeId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class MemberGroupService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMemberGroup(data: MemberGroupData['payloads']['GetItemMemberGroup'] = {}): CancelablePromise<MemberGroupData['responses']['GetItemMemberGroup']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/member-group',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMemberGroup(data: MemberGroupData['payloads']['GetMemberGroup'] = {}): CancelablePromise<MemberGroupData['responses']['GetMemberGroup']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/member-group',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMemberGroup(data: MemberGroupData['payloads']['PostMemberGroup'] = {}): CancelablePromise<MemberGroupData['responses']['PostMemberGroup']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/member-group',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMemberGroupById(data: MemberGroupData['payloads']['GetMemberGroupById']): CancelablePromise<MemberGroupData['responses']['GetMemberGroupById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/member-group/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteMemberGroupById(data: MemberGroupData['payloads']['DeleteMemberGroupById']): CancelablePromise<MemberGroupData['responses']['DeleteMemberGroupById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/member-group/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMemberGroupById(data: MemberGroupData['payloads']['PutMemberGroupById']): CancelablePromise<MemberGroupData['responses']['PutMemberGroupById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/member-group/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeMemberGroupRoot(data: MemberGroupData['payloads']['GetTreeMemberGroupRoot'] = {}): CancelablePromise<MemberGroupData['responses']['GetTreeMemberGroupRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/member-group/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class MemberTypeService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMemberType(data: MemberTypeData['payloads']['GetItemMemberType'] = {}): CancelablePromise<MemberTypeData['responses']['GetItemMemberType']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/member-type',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMemberTypeSearch(data: MemberTypeData['payloads']['GetItemMemberTypeSearch'] = {}): CancelablePromise<MemberTypeData['responses']['GetItemMemberTypeSearch']> {
		const {
                    
                    query,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/member-type/search',
			query: {
				query, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMemberType(data: MemberTypeData['payloads']['PostMemberType'] = {}): CancelablePromise<MemberTypeData['responses']['PostMemberType']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/member-type',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMemberTypeById(data: MemberTypeData['payloads']['GetMemberTypeById']): CancelablePromise<MemberTypeData['responses']['GetMemberTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/member-type/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteMemberTypeById(data: MemberTypeData['payloads']['DeleteMemberTypeById']): CancelablePromise<MemberTypeData['responses']['DeleteMemberTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/member-type/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMemberTypeById(data: MemberTypeData['payloads']['PutMemberTypeById']): CancelablePromise<MemberTypeData['responses']['PutMemberTypeById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/member-type/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMemberTypeByIdCompositionReferences(data: MemberTypeData['payloads']['GetMemberTypeByIdCompositionReferences']): CancelablePromise<MemberTypeData['responses']['GetMemberTypeByIdCompositionReferences']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/member-type/{id}/composition-references',
			path: {
				id
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMemberTypeByIdCopy(data: MemberTypeData['payloads']['PostMemberTypeByIdCopy']): CancelablePromise<MemberTypeData['responses']['PostMemberTypeByIdCopy']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/member-type/{id}/copy',
			path: {
				id
			},
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postMemberTypeAvailableCompositions(data: MemberTypeData['payloads']['PostMemberTypeAvailableCompositions'] = {}): CancelablePromise<MemberTypeData['responses']['PostMemberTypeAvailableCompositions']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/member-type/available-compositions',
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeMemberTypeRoot(data: MemberTypeData['payloads']['GetTreeMemberTypeRoot'] = {}): CancelablePromise<MemberTypeData['responses']['GetTreeMemberTypeRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/member-type/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class MemberService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getFilterMember(data: MemberData['payloads']['GetFilterMember'] = {}): CancelablePromise<MemberData['responses']['GetFilterMember']> {
		const {
                    
                    memberTypeId,
memberGroupName,
isApproved,
isLockedOut,
orderBy,
orderDirection,
filter,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/filter/member',
			query: {
				memberTypeId, memberGroupName, isApproved, isLockedOut, orderBy, orderDirection, filter, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMember(data: MemberData['payloads']['GetItemMember'] = {}): CancelablePromise<MemberData['responses']['GetItemMember']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/member',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemMemberSearch(data: MemberData['payloads']['GetItemMemberSearch'] = {}): CancelablePromise<MemberData['responses']['GetItemMemberSearch']> {
		const {
                    
                    query,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/member/search',
			query: {
				query, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postMember(data: MemberData['payloads']['PostMember'] = {}): CancelablePromise<MemberData['responses']['PostMember']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/member',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMemberById(data: MemberData['payloads']['GetMemberById']): CancelablePromise<MemberData['responses']['GetMemberById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/member/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteMemberById(data: MemberData['payloads']['DeleteMemberById']): CancelablePromise<MemberData['responses']['DeleteMemberById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/member/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMemberById(data: MemberData['payloads']['PutMemberById']): CancelablePromise<MemberData['responses']['PutMemberById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/member/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putMemberByIdValidate(data: MemberData['payloads']['PutMemberByIdValidate']): CancelablePromise<MemberData['responses']['PutMemberByIdValidate']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/member/{id}/validate',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getMemberConfiguration(): CancelablePromise<MemberData['responses']['GetMemberConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/member/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postMemberValidate(data: MemberData['payloads']['PostMemberValidate'] = {}): CancelablePromise<MemberData['responses']['PostMemberValidate']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/member/validate',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

}

export class ModelsBuilderService {

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postModelsBuilderBuild(): CancelablePromise<ModelsBuilderData['responses']['PostModelsBuilderBuild']> {
		
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/models-builder/build',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				428: `Precondition Required`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getModelsBuilderDashboard(): CancelablePromise<ModelsBuilderData['responses']['GetModelsBuilderDashboard']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/models-builder/dashboard',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getModelsBuilderStatus(): CancelablePromise<ModelsBuilderData['responses']['GetModelsBuilderStatus']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/models-builder/status',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class ObjectTypesService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getObjectTypes(data: ObjectTypesData['payloads']['GetObjectTypes'] = {}): CancelablePromise<ObjectTypesData['responses']['GetObjectTypes']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/object-types',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class OEmbedService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getOembedQuery(data: OembedData['payloads']['GetOembedQuery'] = {}): CancelablePromise<OembedData['responses']['GetOembedQuery']> {
		const {
                    
                    url,
maxWidth,
maxHeight
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/oembed/query',
			query: {
				url, maxWidth, maxHeight
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class PackageService {

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postPackageByNameRunMigration(data: PackageData['payloads']['PostPackageByNameRunMigration']): CancelablePromise<PackageData['responses']['PostPackageByNameRunMigration']> {
		const {
                    
                    name
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/package/{name}/run-migration',
			path: {
				name
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
				409: `Conflict`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPackageConfiguration(): CancelablePromise<PackageData['responses']['GetPackageConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/package/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPackageCreated(data: PackageData['payloads']['GetPackageCreated'] = {}): CancelablePromise<PackageData['responses']['GetPackageCreated']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/package/created',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postPackageCreated(data: PackageData['payloads']['PostPackageCreated'] = {}): CancelablePromise<PackageData['responses']['PostPackageCreated']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/package/created',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPackageCreatedById(data: PackageData['payloads']['GetPackageCreatedById']): CancelablePromise<PackageData['responses']['GetPackageCreatedById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/package/created/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deletePackageCreatedById(data: PackageData['payloads']['DeletePackageCreatedById']): CancelablePromise<PackageData['responses']['DeletePackageCreatedById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/package/created/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putPackageCreatedById(data: PackageData['payloads']['PutPackageCreatedById']): CancelablePromise<PackageData['responses']['PutPackageCreatedById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/package/created/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPackageCreatedByIdDownload(data: PackageData['payloads']['GetPackageCreatedByIdDownload']): CancelablePromise<PackageData['responses']['GetPackageCreatedByIdDownload']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/package/created/{id}/download',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPackageMigrationStatus(data: PackageData['payloads']['GetPackageMigrationStatus'] = {}): CancelablePromise<PackageData['responses']['GetPackageMigrationStatus']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/package/migration-status',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class PartialViewService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemPartialView(data: PartialViewData['payloads']['GetItemPartialView'] = {}): CancelablePromise<PartialViewData['responses']['GetItemPartialView']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/partial-view',
			query: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postPartialView(data: PartialViewData['payloads']['PostPartialView'] = {}): CancelablePromise<PartialViewData['responses']['PostPartialView']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/partial-view',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPartialViewByPath(data: PartialViewData['payloads']['GetPartialViewByPath']): CancelablePromise<PartialViewData['responses']['GetPartialViewByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/partial-view/{path}',
			path: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deletePartialViewByPath(data: PartialViewData['payloads']['DeletePartialViewByPath']): CancelablePromise<PartialViewData['responses']['DeletePartialViewByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/partial-view/{path}',
			path: {
				path
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putPartialViewByPath(data: PartialViewData['payloads']['PutPartialViewByPath']): CancelablePromise<PartialViewData['responses']['PutPartialViewByPath']> {
		const {
                    
                    path,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/partial-view/{path}',
			path: {
				path
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static putPartialViewByPathRename(data: PartialViewData['payloads']['PutPartialViewByPathRename']): CancelablePromise<PartialViewData['responses']['PutPartialViewByPathRename']> {
		const {
                    
                    path,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/partial-view/{path}/rename',
			path: {
				path
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postPartialViewFolder(data: PartialViewData['payloads']['PostPartialViewFolder'] = {}): CancelablePromise<PartialViewData['responses']['PostPartialViewFolder']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/partial-view/folder',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPartialViewFolderByPath(data: PartialViewData['payloads']['GetPartialViewFolderByPath']): CancelablePromise<PartialViewData['responses']['GetPartialViewFolderByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/partial-view/folder/{path}',
			path: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deletePartialViewFolderByPath(data: PartialViewData['payloads']['DeletePartialViewFolderByPath']): CancelablePromise<PartialViewData['responses']['DeletePartialViewFolderByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/partial-view/folder/{path}',
			path: {
				path
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPartialViewSnippet(data: PartialViewData['payloads']['GetPartialViewSnippet'] = {}): CancelablePromise<PartialViewData['responses']['GetPartialViewSnippet']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/partial-view/snippet',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getPartialViewSnippetById(data: PartialViewData['payloads']['GetPartialViewSnippetById']): CancelablePromise<PartialViewData['responses']['GetPartialViewSnippetById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/partial-view/snippet/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreePartialViewAncestors(data: PartialViewData['payloads']['GetTreePartialViewAncestors'] = {}): CancelablePromise<PartialViewData['responses']['GetTreePartialViewAncestors']> {
		const {
                    
                    descendantPath
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/partial-view/ancestors',
			query: {
				descendantPath
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreePartialViewChildren(data: PartialViewData['payloads']['GetTreePartialViewChildren'] = {}): CancelablePromise<PartialViewData['responses']['GetTreePartialViewChildren']> {
		const {
                    
                    parentPath,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/partial-view/children',
			query: {
				parentPath, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreePartialViewRoot(data: PartialViewData['payloads']['GetTreePartialViewRoot'] = {}): CancelablePromise<PartialViewData['responses']['GetTreePartialViewRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/partial-view/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class PreviewService {

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deletePreview(): CancelablePromise<PreviewData['responses']['DeletePreview']> {
		
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/preview',
			responseHeader: 'Umb-Notifications',
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postPreview(): CancelablePromise<PreviewData['responses']['PostPreview']> {
		
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/preview',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class ProfilingService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getProfilingStatus(): CancelablePromise<ProfilingData['responses']['GetProfilingStatus']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/profiling/status',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putProfilingStatus(data: ProfilingData['payloads']['PutProfilingStatus'] = {}): CancelablePromise<ProfilingData['responses']['PutProfilingStatus']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/profiling/status',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class PropertyTypeService {

	/**
	 * @returns boolean OK
	 * @throws ApiError
	 */
	public static getPropertyTypeIsUsed(data: PropertyTypeData['payloads']['GetPropertyTypeIsUsed'] = {}): CancelablePromise<PropertyTypeData['responses']['GetPropertyTypeIsUsed']> {
		const {
                    
                    contentTypeId,
propertyAlias
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/property-type/is-used',
			query: {
				contentTypeId, propertyAlias
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class PublishedCacheService {

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postPublishedCacheCollect(): CancelablePromise<PublishedCacheData['responses']['PostPublishedCacheCollect']> {
		
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/published-cache/collect',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postPublishedCacheRebuild(): CancelablePromise<PublishedCacheData['responses']['PostPublishedCacheRebuild']> {
		
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/published-cache/rebuild',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postPublishedCacheReload(): CancelablePromise<PublishedCacheData['responses']['PostPublishedCacheReload']> {
		
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/published-cache/reload',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static getPublishedCacheStatus(): CancelablePromise<PublishedCacheData['responses']['GetPublishedCacheStatus']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/published-cache/status',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class RedirectManagementService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRedirectManagement(data: RedirectManagementData['payloads']['GetRedirectManagement'] = {}): CancelablePromise<RedirectManagementData['responses']['GetRedirectManagement']> {
		const {
                    
                    filter,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/redirect-management',
			query: {
				filter, skip, take
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRedirectManagementById(data: RedirectManagementData['payloads']['GetRedirectManagementById']): CancelablePromise<RedirectManagementData['responses']['GetRedirectManagementById']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/redirect-management/{id}',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteRedirectManagementById(data: RedirectManagementData['payloads']['DeleteRedirectManagementById']): CancelablePromise<RedirectManagementData['responses']['DeleteRedirectManagementById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/redirect-management/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRedirectManagementStatus(): CancelablePromise<RedirectManagementData['responses']['GetRedirectManagementStatus']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/redirect-management/status',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postRedirectManagementStatus(data: RedirectManagementData['payloads']['PostRedirectManagementStatus'] = {}): CancelablePromise<RedirectManagementData['responses']['PostRedirectManagementStatus']> {
		const {
                    
                    status
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/redirect-management/status',
			query: {
				status
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class RelationTypeService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemRelationType(data: RelationTypeData['payloads']['GetItemRelationType'] = {}): CancelablePromise<RelationTypeData['responses']['GetItemRelationType']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/relation-type',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRelationType(data: RelationTypeData['payloads']['GetRelationType'] = {}): CancelablePromise<RelationTypeData['responses']['GetRelationType']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/relation-type',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRelationTypeById(data: RelationTypeData['payloads']['GetRelationTypeById']): CancelablePromise<RelationTypeData['responses']['GetRelationTypeById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/relation-type/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

}

export class RelationService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getRelationTypeById(data: RelationData['payloads']['GetRelationTypeById']): CancelablePromise<RelationData['responses']['GetRelationTypeById']> {
		const {
                    
                    id,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/relation/type/{id}',
			path: {
				id
			},
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

}

export class ScriptService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemScript(data: ScriptData['payloads']['GetItemScript'] = {}): CancelablePromise<ScriptData['responses']['GetItemScript']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/script',
			query: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postScript(data: ScriptData['payloads']['PostScript'] = {}): CancelablePromise<ScriptData['responses']['PostScript']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/script',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getScriptByPath(data: ScriptData['payloads']['GetScriptByPath']): CancelablePromise<ScriptData['responses']['GetScriptByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/script/{path}',
			path: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteScriptByPath(data: ScriptData['payloads']['DeleteScriptByPath']): CancelablePromise<ScriptData['responses']['DeleteScriptByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/script/{path}',
			path: {
				path
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putScriptByPath(data: ScriptData['payloads']['PutScriptByPath']): CancelablePromise<ScriptData['responses']['PutScriptByPath']> {
		const {
                    
                    path,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/script/{path}',
			path: {
				path
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static putScriptByPathRename(data: ScriptData['payloads']['PutScriptByPathRename']): CancelablePromise<ScriptData['responses']['PutScriptByPathRename']> {
		const {
                    
                    path,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/script/{path}/rename',
			path: {
				path
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postScriptFolder(data: ScriptData['payloads']['PostScriptFolder'] = {}): CancelablePromise<ScriptData['responses']['PostScriptFolder']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/script/folder',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getScriptFolderByPath(data: ScriptData['payloads']['GetScriptFolderByPath']): CancelablePromise<ScriptData['responses']['GetScriptFolderByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/script/folder/{path}',
			path: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteScriptFolderByPath(data: ScriptData['payloads']['DeleteScriptFolderByPath']): CancelablePromise<ScriptData['responses']['DeleteScriptFolderByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/script/folder/{path}',
			path: {
				path
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeScriptAncestors(data: ScriptData['payloads']['GetTreeScriptAncestors'] = {}): CancelablePromise<ScriptData['responses']['GetTreeScriptAncestors']> {
		const {
                    
                    descendantPath
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/script/ancestors',
			query: {
				descendantPath
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeScriptChildren(data: ScriptData['payloads']['GetTreeScriptChildren'] = {}): CancelablePromise<ScriptData['responses']['GetTreeScriptChildren']> {
		const {
                    
                    parentPath,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/script/children',
			query: {
				parentPath, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeScriptRoot(data: ScriptData['payloads']['GetTreeScriptRoot'] = {}): CancelablePromise<ScriptData['responses']['GetTreeScriptRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/script/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class SearcherService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getSearcher(data: SearcherData['payloads']['GetSearcher'] = {}): CancelablePromise<SearcherData['responses']['GetSearcher']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/searcher',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getSearcherBySearcherNameQuery(data: SearcherData['payloads']['GetSearcherBySearcherNameQuery']): CancelablePromise<SearcherData['responses']['GetSearcherBySearcherNameQuery']> {
		const {
                    
                    searcherName,
term,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/searcher/{searcherName}/query',
			path: {
				searcherName
			},
			query: {
				term, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

}

export class SecurityService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getSecurityConfiguration(): CancelablePromise<SecurityData['responses']['GetSecurityConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/security/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postSecurityForgotPassword(data: SecurityData['payloads']['PostSecurityForgotPassword'] = {}): CancelablePromise<SecurityData['responses']['PostSecurityForgotPassword']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/security/forgot-password',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string No Content
	 * @throws ApiError
	 */
	public static postSecurityForgotPasswordReset(data: SecurityData['payloads']['PostSecurityForgotPasswordReset'] = {}): CancelablePromise<SecurityData['responses']['PostSecurityForgotPasswordReset']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/security/forgot-password/reset',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postSecurityForgotPasswordVerify(data: SecurityData['payloads']['PostSecurityForgotPasswordVerify'] = {}): CancelablePromise<SecurityData['responses']['PostSecurityForgotPasswordVerify']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/security/forgot-password/verify',
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				400: `Bad Request`,
				404: `Not Found`,
			},
		});
	}

}

export class SegmentService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getSegment(data: SegmentData['payloads']['GetSegment'] = {}): CancelablePromise<SegmentData['responses']['GetSegment']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/segment',
			query: {
				skip, take
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class ServerService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getServerConfiguration(): CancelablePromise<ServerData['responses']['GetServerConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/server/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getServerInformation(): CancelablePromise<ServerData['responses']['GetServerInformation']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/server/information',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getServerStatus(): CancelablePromise<ServerData['responses']['GetServerStatus']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/server/status',
			errors: {
				400: `Bad Request`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getServerTroubleshooting(): CancelablePromise<ServerData['responses']['GetServerTroubleshooting']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/server/troubleshooting',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class StaticFileService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemStaticFile(data: StaticFileData['payloads']['GetItemStaticFile'] = {}): CancelablePromise<StaticFileData['responses']['GetItemStaticFile']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/static-file',
			query: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeStaticFileAncestors(data: StaticFileData['payloads']['GetTreeStaticFileAncestors'] = {}): CancelablePromise<StaticFileData['responses']['GetTreeStaticFileAncestors']> {
		const {
                    
                    descendantPath
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/static-file/ancestors',
			query: {
				descendantPath
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeStaticFileChildren(data: StaticFileData['payloads']['GetTreeStaticFileChildren'] = {}): CancelablePromise<StaticFileData['responses']['GetTreeStaticFileChildren']> {
		const {
                    
                    parentPath,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/static-file/children',
			query: {
				parentPath, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeStaticFileRoot(data: StaticFileData['payloads']['GetTreeStaticFileRoot'] = {}): CancelablePromise<StaticFileData['responses']['GetTreeStaticFileRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/static-file/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class StylesheetService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemStylesheet(data: StylesheetData['payloads']['GetItemStylesheet'] = {}): CancelablePromise<StylesheetData['responses']['GetItemStylesheet']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/stylesheet',
			query: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postStylesheet(data: StylesheetData['payloads']['PostStylesheet'] = {}): CancelablePromise<StylesheetData['responses']['PostStylesheet']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/stylesheet',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getStylesheetByPath(data: StylesheetData['payloads']['GetStylesheetByPath']): CancelablePromise<StylesheetData['responses']['GetStylesheetByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/stylesheet/{path}',
			path: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteStylesheetByPath(data: StylesheetData['payloads']['DeleteStylesheetByPath']): CancelablePromise<StylesheetData['responses']['DeleteStylesheetByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/stylesheet/{path}',
			path: {
				path
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putStylesheetByPath(data: StylesheetData['payloads']['PutStylesheetByPath']): CancelablePromise<StylesheetData['responses']['PutStylesheetByPath']> {
		const {
                    
                    path,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/stylesheet/{path}',
			path: {
				path
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static putStylesheetByPathRename(data: StylesheetData['payloads']['PutStylesheetByPathRename']): CancelablePromise<StylesheetData['responses']['PutStylesheetByPathRename']> {
		const {
                    
                    path,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/stylesheet/{path}/rename',
			path: {
				path
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postStylesheetFolder(data: StylesheetData['payloads']['PostStylesheetFolder'] = {}): CancelablePromise<StylesheetData['responses']['PostStylesheetFolder']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/stylesheet/folder',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getStylesheetFolderByPath(data: StylesheetData['payloads']['GetStylesheetFolderByPath']): CancelablePromise<StylesheetData['responses']['GetStylesheetFolderByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/stylesheet/folder/{path}',
			path: {
				path
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteStylesheetFolderByPath(data: StylesheetData['payloads']['DeleteStylesheetFolderByPath']): CancelablePromise<StylesheetData['responses']['DeleteStylesheetFolderByPath']> {
		const {
                    
                    path
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/stylesheet/folder/{path}',
			path: {
				path
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeStylesheetAncestors(data: StylesheetData['payloads']['GetTreeStylesheetAncestors'] = {}): CancelablePromise<StylesheetData['responses']['GetTreeStylesheetAncestors']> {
		const {
                    
                    descendantPath
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/stylesheet/ancestors',
			query: {
				descendantPath
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeStylesheetChildren(data: StylesheetData['payloads']['GetTreeStylesheetChildren'] = {}): CancelablePromise<StylesheetData['responses']['GetTreeStylesheetChildren']> {
		const {
                    
                    parentPath,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/stylesheet/children',
			query: {
				parentPath, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeStylesheetRoot(data: StylesheetData['payloads']['GetTreeStylesheetRoot'] = {}): CancelablePromise<StylesheetData['responses']['GetTreeStylesheetRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/stylesheet/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class TagService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTag(data: TagData['payloads']['GetTag'] = {}): CancelablePromise<TagData['responses']['GetTag']> {
		const {
                    
                    query,
tagGroup,
culture,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tag',
			query: {
				query, tagGroup, culture, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class TelemetryService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTelemetry(data: TelemetryData['payloads']['GetTelemetry'] = {}): CancelablePromise<TelemetryData['responses']['GetTelemetry']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/telemetry',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTelemetryLevel(): CancelablePromise<TelemetryData['responses']['GetTelemetryLevel']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/telemetry/level',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postTelemetryLevel(data: TelemetryData['payloads']['PostTelemetryLevel'] = {}): CancelablePromise<TelemetryData['responses']['PostTelemetryLevel']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/telemetry/level',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class TemplateService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemTemplate(data: TemplateData['payloads']['GetItemTemplate'] = {}): CancelablePromise<TemplateData['responses']['GetItemTemplate']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/template',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemTemplateSearch(data: TemplateData['payloads']['GetItemTemplateSearch'] = {}): CancelablePromise<TemplateData['responses']['GetItemTemplateSearch']> {
		const {
                    
                    query,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/template/search',
			query: {
				query, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postTemplate(data: TemplateData['payloads']['PostTemplate'] = {}): CancelablePromise<TemplateData['responses']['PostTemplate']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/template',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTemplateById(data: TemplateData['payloads']['GetTemplateById']): CancelablePromise<TemplateData['responses']['GetTemplateById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/template/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteTemplateById(data: TemplateData['payloads']['DeleteTemplateById']): CancelablePromise<TemplateData['responses']['DeleteTemplateById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/template/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putTemplateById(data: TemplateData['payloads']['PutTemplateById']): CancelablePromise<TemplateData['responses']['PutTemplateById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/template/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTemplateConfiguration(): CancelablePromise<TemplateData['responses']['GetTemplateConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/template/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postTemplateQueryExecute(data: TemplateData['payloads']['PostTemplateQueryExecute'] = {}): CancelablePromise<TemplateData['responses']['PostTemplateQueryExecute']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/template/query/execute',
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTemplateQuerySettings(): CancelablePromise<TemplateData['responses']['GetTemplateQuerySettings']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/template/query/settings',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeTemplateAncestors(data: TemplateData['payloads']['GetTreeTemplateAncestors'] = {}): CancelablePromise<TemplateData['responses']['GetTreeTemplateAncestors']> {
		const {
                    
                    descendantId
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/template/ancestors',
			query: {
				descendantId
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeTemplateChildren(data: TemplateData['payloads']['GetTreeTemplateChildren'] = {}): CancelablePromise<TemplateData['responses']['GetTreeTemplateChildren']> {
		const {
                    
                    parentId,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/template/children',
			query: {
				parentId, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTreeTemplateRoot(data: TemplateData['payloads']['GetTreeTemplateRoot'] = {}): CancelablePromise<TemplateData['responses']['GetTreeTemplateRoot']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/tree/template/root',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class TemporaryFileService {

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postTemporaryFile(data: TemporaryFileData['payloads']['PostTemporaryFile'] = {}): CancelablePromise<TemporaryFileData['responses']['PostTemporaryFile']> {
		const {
                    
                    formData
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/temporary-file',
			formData: formData,
			mediaType: 'multipart/form-data',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTemporaryFileById(data: TemporaryFileData['payloads']['GetTemporaryFileById']): CancelablePromise<TemporaryFileData['responses']['GetTemporaryFileById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/temporary-file/{id}',
			path: {
				id
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteTemporaryFileById(data: TemporaryFileData['payloads']['DeleteTemporaryFileById']): CancelablePromise<TemporaryFileData['responses']['DeleteTemporaryFileById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/temporary-file/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getTemporaryFileConfiguration(): CancelablePromise<TemporaryFileData['responses']['GetTemporaryFileConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/temporary-file/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}

export class UpgradeService {

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUpgradeAuthorize(): CancelablePromise<UpgradeData['responses']['PostUpgradeAuthorize']> {
		
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/upgrade/authorize',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				428: `Precondition Required`,
				500: `Internal Server Error`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUpgradeSettings(): CancelablePromise<UpgradeData['responses']['GetUpgradeSettings']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/upgrade/settings',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				428: `Precondition Required`,
			},
		});
	}

}

export class UserDataService {

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postUserData(data: UserDataData['payloads']['PostUserData'] = {}): CancelablePromise<UserDataData['responses']['PostUserData']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user-data',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserData(data: UserDataData['payloads']['GetUserData'] = {}): CancelablePromise<UserDataData['responses']['GetUserData']> {
		const {
                    
                    groups,
identifiers,
skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user-data',
			query: {
				groups, identifiers, skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putUserData(data: UserDataData['payloads']['PutUserData'] = {}): CancelablePromise<UserDataData['responses']['PutUserData']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/user-data',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserDataById(data: UserDataData['payloads']['GetUserDataById']): CancelablePromise<UserDataData['responses']['GetUserDataById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user-data/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

}

export class UserGroupService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getFilterUserGroup(data: UserGroupData['payloads']['GetFilterUserGroup'] = {}): CancelablePromise<UserGroupData['responses']['GetFilterUserGroup']> {
		const {
                    
                    skip,
take,
filter
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/filter/user-group',
			query: {
				skip, take, filter
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemUserGroup(data: UserGroupData['payloads']['GetItemUserGroup'] = {}): CancelablePromise<UserGroupData['responses']['GetItemUserGroup']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/user-group',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteUserGroup(data: UserGroupData['payloads']['DeleteUserGroup'] = {}): CancelablePromise<UserGroupData['responses']['DeleteUserGroup']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/user-group',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postUserGroup(data: UserGroupData['payloads']['PostUserGroup'] = {}): CancelablePromise<UserGroupData['responses']['PostUserGroup']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user-group',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserGroup(data: UserGroupData['payloads']['GetUserGroup'] = {}): CancelablePromise<UserGroupData['responses']['GetUserGroup']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user-group',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserGroupById(data: UserGroupData['payloads']['GetUserGroupById']): CancelablePromise<UserGroupData['responses']['GetUserGroupById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user-group/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteUserGroupById(data: UserGroupData['payloads']['DeleteUserGroupById']): CancelablePromise<UserGroupData['responses']['DeleteUserGroupById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/user-group/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putUserGroupById(data: UserGroupData['payloads']['PutUserGroupById']): CancelablePromise<UserGroupData['responses']['PutUserGroupById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/user-group/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteUserGroupByIdUsers(data: UserGroupData['payloads']['DeleteUserGroupByIdUsers']): CancelablePromise<UserGroupData['responses']['DeleteUserGroupByIdUsers']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/user-group/{id}/users',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserGroupByIdUsers(data: UserGroupData['payloads']['PostUserGroupByIdUsers']): CancelablePromise<UserGroupData['responses']['PostUserGroupByIdUsers']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user-group/{id}/users',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

}

export class UserService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getFilterUser(data: UserData['payloads']['GetFilterUser'] = {}): CancelablePromise<UserData['responses']['GetFilterUser']> {
		const {
                    
                    skip,
take,
orderBy,
orderDirection,
userGroupIds,
userStates,
filter
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/filter/user',
			query: {
				skip, take, orderBy, orderDirection, userGroupIds, userStates, filter
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemUser(data: UserData['payloads']['GetItemUser'] = {}): CancelablePromise<UserData['responses']['GetItemUser']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/user',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postUser(data: UserData['payloads']['PostUser'] = {}): CancelablePromise<UserData['responses']['PostUser']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteUser(data: UserData['payloads']['DeleteUser'] = {}): CancelablePromise<UserData['responses']['DeleteUser']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/user',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUser(data: UserData['payloads']['GetUser'] = {}): CancelablePromise<UserData['responses']['GetUser']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserById(data: UserData['payloads']['GetUserById']): CancelablePromise<UserData['responses']['GetUserById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteUserById(data: UserData['payloads']['DeleteUserById']): CancelablePromise<UserData['responses']['DeleteUserById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/user/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putUserById(data: UserData['payloads']['PutUserById']): CancelablePromise<UserData['responses']['PutUserById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/user/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserById2Fa(data: UserData['payloads']['GetUserById2Fa']): CancelablePromise<UserData['responses']['GetUserById2Fa']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/{id}/2fa',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteUserById2FaByProviderName(data: UserData['payloads']['DeleteUserById2FaByProviderName']): CancelablePromise<UserData['responses']['DeleteUserById2FaByProviderName']> {
		const {
                    
                    id,
providerName
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/user/{id}/2fa/{providerName}',
			path: {
				id, providerName
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserByIdChangePassword(data: UserData['payloads']['PostUserByIdChangePassword']): CancelablePromise<UserData['responses']['PostUserByIdChangePassword']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/{id}/change-password',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postUserByIdResetPassword(data: UserData['payloads']['PostUserByIdResetPassword']): CancelablePromise<UserData['responses']['PostUserByIdResetPassword']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/{id}/reset-password',
			path: {
				id
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteUserAvatarById(data: UserData['payloads']['DeleteUserAvatarById']): CancelablePromise<UserData['responses']['DeleteUserAvatarById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/user/avatar/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserAvatarById(data: UserData['payloads']['PostUserAvatarById']): CancelablePromise<UserData['responses']['PostUserAvatarById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/avatar/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserConfiguration(): CancelablePromise<UserData['responses']['GetUserConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserCurrent(): CancelablePromise<UserData['responses']['GetUserCurrent']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/current',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserCurrent2Fa(): CancelablePromise<UserData['responses']['GetUserCurrent2Fa']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/current/2fa',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteUserCurrent2FaByProviderName(data: UserData['payloads']['DeleteUserCurrent2FaByProviderName']): CancelablePromise<UserData['responses']['DeleteUserCurrent2FaByProviderName']> {
		const {
                    
                    providerName,
code
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/user/current/2fa/{providerName}',
			path: {
				providerName
			},
			query: {
				code
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postUserCurrent2FaByProviderName(data: UserData['payloads']['PostUserCurrent2FaByProviderName']): CancelablePromise<UserData['responses']['PostUserCurrent2FaByProviderName']> {
		const {
                    
                    providerName,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/current/2fa/{providerName}',
			path: {
				providerName
			},
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserCurrent2FaByProviderName(data: UserData['payloads']['GetUserCurrent2FaByProviderName']): CancelablePromise<UserData['responses']['GetUserCurrent2FaByProviderName']> {
		const {
                    
                    providerName
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/current/2fa/{providerName}',
			path: {
				providerName
			},
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserCurrentAvatar(data: UserData['payloads']['PostUserCurrentAvatar'] = {}): CancelablePromise<UserData['responses']['PostUserCurrentAvatar']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/current/avatar',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserCurrentChangePassword(data: UserData['payloads']['PostUserCurrentChangePassword'] = {}): CancelablePromise<UserData['responses']['PostUserCurrentChangePassword']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/current/change-password',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserCurrentConfiguration(): CancelablePromise<UserData['responses']['GetUserCurrentConfiguration']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/current/configuration',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserCurrentLoginProviders(): CancelablePromise<UserData['responses']['GetUserCurrentLoginProviders']> {
		
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/current/login-providers',
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserCurrentPermissions(data: UserData['payloads']['GetUserCurrentPermissions'] = {}): CancelablePromise<UserData['responses']['GetUserCurrentPermissions']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/current/permissions',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserCurrentPermissionsDocument(data: UserData['payloads']['GetUserCurrentPermissionsDocument'] = {}): CancelablePromise<UserData['responses']['GetUserCurrentPermissionsDocument']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/current/permissions/document',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getUserCurrentPermissionsMedia(data: UserData['payloads']['GetUserCurrentPermissionsMedia'] = {}): CancelablePromise<UserData['responses']['GetUserCurrentPermissionsMedia']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/user/current/permissions/media',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserDisable(data: UserData['payloads']['PostUserDisable'] = {}): CancelablePromise<UserData['responses']['PostUserDisable']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/disable',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserEnable(data: UserData['payloads']['PostUserEnable'] = {}): CancelablePromise<UserData['responses']['PostUserEnable']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/enable',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postUserInvite(data: UserData['payloads']['PostUserInvite'] = {}): CancelablePromise<UserData['responses']['PostUserInvite']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/invite',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserInviteCreatePassword(data: UserData['payloads']['PostUserInviteCreatePassword'] = {}): CancelablePromise<UserData['responses']['PostUserInviteCreatePassword']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/invite/create-password',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserInviteResend(data: UserData['payloads']['PostUserInviteResend'] = {}): CancelablePromise<UserData['responses']['PostUserInviteResend']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/invite/resend',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static postUserInviteVerify(data: UserData['payloads']['PostUserInviteVerify'] = {}): CancelablePromise<UserData['responses']['PostUserInviteVerify']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/invite/verify',
			body: requestBody,
			mediaType: 'application/json',
			errors: {
				400: `Bad Request`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserSetUserGroups(data: UserData['payloads']['PostUserSetUserGroups'] = {}): CancelablePromise<UserData['responses']['PostUserSetUserGroups']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/set-user-groups',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static postUserUnlock(data: UserData['payloads']['PostUserUnlock'] = {}): CancelablePromise<UserData['responses']['PostUserUnlock']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/user/unlock',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
			},
		});
	}

}

export class WebhookService {

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getItemWebhook(data: WebhookData['payloads']['GetItemWebhook'] = {}): CancelablePromise<WebhookData['responses']['GetItemWebhook']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/item/webhook',
			query: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getWebhook(data: WebhookData['payloads']['GetWebhook'] = {}): CancelablePromise<WebhookData['responses']['GetWebhook']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/webhook',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

	/**
	 * @returns string Created
	 * @throws ApiError
	 */
	public static postWebhook(data: WebhookData['payloads']['PostWebhook'] = {}): CancelablePromise<WebhookData['responses']['PostWebhook']> {
		const {
                    
                    requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'POST',
			url: '/umbraco/management/api/v1/webhook',
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Generated-Resource',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getWebhookById(data: WebhookData['payloads']['GetWebhookById']): CancelablePromise<WebhookData['responses']['GetWebhookById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/webhook/{id}',
			path: {
				id
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static deleteWebhookById(data: WebhookData['payloads']['DeleteWebhookById']): CancelablePromise<WebhookData['responses']['DeleteWebhookById']> {
		const {
                    
                    id
                } = data;
		return __request(OpenAPI, {
			method: 'DELETE',
			url: '/umbraco/management/api/v1/webhook/{id}',
			path: {
				id
			},
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns string OK
	 * @throws ApiError
	 */
	public static putWebhookById(data: WebhookData['payloads']['PutWebhookById']): CancelablePromise<WebhookData['responses']['PutWebhookById']> {
		const {
                    
                    id,
requestBody
                } = data;
		return __request(OpenAPI, {
			method: 'PUT',
			url: '/umbraco/management/api/v1/webhook/{id}',
			path: {
				id
			},
			body: requestBody,
			mediaType: 'application/json',
			responseHeader: 'Umb-Notifications',
			errors: {
				400: `Bad Request`,
				401: `The resource is protected and requires an authentication token`,
				403: `The authenticated user do not have access to this resource`,
				404: `Not Found`,
			},
		});
	}

	/**
	 * @returns unknown OK
	 * @throws ApiError
	 */
	public static getWebhookEvents(data: WebhookData['payloads']['GetWebhookEvents'] = {}): CancelablePromise<WebhookData['responses']['GetWebhookEvents']> {
		const {
                    
                    skip,
take
                } = data;
		return __request(OpenAPI, {
			method: 'GET',
			url: '/umbraco/management/api/v1/webhook/events',
			query: {
				skip, take
			},
			errors: {
				401: `The resource is protected and requires an authentication token`,
			},
		});
	}

}