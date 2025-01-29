import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityFolderManager } from '../utils/entity/entity-folder.manager.js';
import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import type { UmbMockMediaTypeModel, UmbMockMediaTypeUnionModel } from './media-type.data.js';
import { data } from './media-type.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	AllowedMediaTypeModel,
	CreateFolderRequestModel,
	CreateMediaTypeRequestModel,
	GetItemMediaTypeAllowedResponse,
	MediaTypeItemResponseModel,
	MediaTypeResponseModel,
	MediaTypeSortModel,
	MediaTypeTreeItemResponseModel,
	PagedAllowedMediaTypeModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbDataTypeMockDb } from '../data-type/data-type.db.js';

class UmbMediaTypeMockDB extends UmbEntityMockDbBase<UmbMockMediaTypeModel> {
	tree = new UmbMockEntityTreeManager<UmbMockMediaTypeModel>(this, mediaTypeTreeItemMapper);
	folder = new UmbMockEntityFolderManager<UmbMockMediaTypeModel>(this, createMockMediaTypeFolderMapper);
	item = new UmbMockEntityItemManager<UmbMockMediaTypeModel>(this, mediaTypeItemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMediaTypeModel>(
		this,
		createMockMediaTypeMapper,
		mediaTypeDetailMapper,
	);

	constructor(data: Array<UmbMockMediaTypeModel>) {
		super(data);
	}

	getAllowedChildren(id: string): PagedAllowedMediaTypeModel {
		const mediaType = this.detail.read(id);
		const allowedMediaTypes = mediaType.allowedMediaTypes.map((sortModel: MediaTypeSortModel) =>
			this.detail.read(sortModel.mediaType.id),
		);
		const mappedItems = allowedMediaTypes.map((item: UmbMockMediaTypeModel) => allowedMediaTypeMapper(item));
		return { items: mappedItems, total: mappedItems.length };
	}

	getAllowedAtRoot(): PagedAllowedMediaTypeModel {
		const mockItems = this.data.filter((item) => item.allowedAsRoot);
		const mappedItems = mockItems.map((item) => allowedMediaTypeMapper(item));
		return { items: mappedItems, total: mappedItems.length };
	}

	getAllowedByFileExtension(fileExtension: string): GetItemMediaTypeAllowedResponse {
		const allowedTypes = this.data.filter((field) => {
			const allProperties = field.properties.flat();

			const fileUploadType = allProperties.find((prop) => prop.alias === 'umbracoFile' || prop.alias === 'mediaPicker');
			if (!fileUploadType) return false;

			const dataType = umbDataTypeMockDb.read(fileUploadType.dataType.id);
			if (dataType?.editorAlias !== 'Umbraco.UploadField') return false;

			const allowedFileExtensions = dataType.values.find((value) => value.alias === 'fileExtensions')?.value;
			if (!allowedFileExtensions || !Array.isArray(allowedFileExtensions)) return false;

			return allowedFileExtensions.includes(fileExtension);
		});

		const mappedTypes = allowedTypes.map(mediaTypeItemMapper);
		return allowedExtensionMediaTypeMapper(mappedTypes, mappedTypes.length);
	}
}

const createMockMediaTypeFolderMapper = (request: CreateFolderRequestModel): UmbMockMediaTypeModel => {
	return {
		name: request.name,
		id: request.id ? request.id : UmbId.new(),
		parent: request.parent,
		description: '',
		alias: '',
		icon: '',
		properties: [],
		containers: [],
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		allowedMediaTypes: [],
		compositions: [],
		isFolder: true,
		hasChildren: false,
		collection: null,
		isDeletable: false,
		aliasCanBeChanged: false,
	};
};

const createMockMediaTypeMapper = (request: CreateMediaTypeRequestModel): UmbMockMediaTypeModel => {
	return {
		name: request.name,
		id: request.id ? request.id : UmbId.new(),
		description: request.description,
		alias: request.alias,
		icon: request.icon,
		properties: request.properties,
		containers: request.containers,
		allowedAsRoot: request.allowedAsRoot,
		variesByCulture: request.variesByCulture,
		variesBySegment: request.variesBySegment,
		isElement: request.isElement,
		allowedMediaTypes: request.allowedMediaTypes,
		compositions: request.compositions,
		parent: request.parent ? { id: request.parent.id } : null,
		isFolder: false,
		hasChildren: false,
		collection: null,
		isDeletable: false,
		aliasCanBeChanged: false,
	};
};

const mediaTypeDetailMapper = (item: UmbMockMediaTypeModel): MediaTypeResponseModel => {
	return {
		name: item.name,
		id: item.id,
		description: item.description,
		alias: item.alias,
		icon: item.icon,
		properties: item.properties,
		containers: item.containers,
		allowedAsRoot: item.allowedAsRoot,
		variesByCulture: item.variesByCulture,
		variesBySegment: item.variesBySegment,
		isElement: item.isElement,
		allowedMediaTypes: item.allowedMediaTypes,
		compositions: item.compositions,
		collection: item.collection,
		isDeletable: item.isDeletable,
		aliasCanBeChanged: item.aliasCanBeChanged,
	};
};

const mediaTypeTreeItemMapper = (item: UmbMockMediaTypeModel): MediaTypeTreeItemResponseModel => {
	return {
		name: item.name,
		hasChildren: item.hasChildren,
		id: item.id,
		parent: item.parent,
		isFolder: item.isFolder,
		icon: item.icon,
		isDeletable: item.isDeletable,
	};
};

const mediaTypeItemMapper = (item: UmbMockMediaTypeUnionModel): MediaTypeItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		icon: item.icon,
	};
};

const allowedMediaTypeMapper = (item: UmbMockMediaTypeModel): AllowedMediaTypeModel => {
	return {
		id: item.id,
		name: item.name,
		description: item.description,
		icon: item.icon,
	};
};

const allowedExtensionMediaTypeMapper = (
	items: Array<MediaTypeItemResponseModel>,
	total: number,
): GetItemMediaTypeAllowedResponse => {
	return {
		items,
		total,
	};
};

export const umbMediaTypeMockDb = new UmbMediaTypeMockDB(data);
