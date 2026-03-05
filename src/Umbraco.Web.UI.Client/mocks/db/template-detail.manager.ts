import type { UmbMockTemplateModel } from '../data/types/mock-data-set.types.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import type { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import type { CreateTemplateRequestModel, TemplateResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbMockTemplateDetailManager extends UmbMockEntityDetailManager<UmbMockTemplateModel> {
	constructor(db: UmbEntityMockDbBase<UmbMockTemplateModel>) {
		super(db, createDetailMockMapper, detailResponseMapper);
	}

	createScaffold(masterTemplateAlias: string = 'master') {
		return `@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@{
		Layout = ${masterTemplateAlias};
}`;
	}
}

const createDetailMockMapper = (request: CreateTemplateRequestModel): UmbMockTemplateModel => {
	return {
		id: UmbId.new(),
		parent: null,
		name: request.name,
		hasChildren: false,
		alias: request.alias,
		content: request.content,
		flags: [],
	};
};

const detailResponseMapper = (item: UmbMockTemplateModel): TemplateResponseModel => {
	return {
		id: item.id,
		name: item.name,
		alias: item.alias,
		content: item.content,
	};
};
