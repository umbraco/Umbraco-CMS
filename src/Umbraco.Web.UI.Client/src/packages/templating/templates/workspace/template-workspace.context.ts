import type { UmbTemplateDetailModel } from '../types.js';
import type { UmbTemplateItemModel, UmbTemplateDetailRepository } from '../repository/index.js';
import { UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS, UmbTemplateItemRepository } from '../repository/index.js';
import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import { UMB_TEMPLATE_WORKSPACE_ALIAS } from './manifests.js';
import { UmbTemplateWorkspaceEditorElement } from './template-workspace-editor.element.js';
import type { UmbRoutableWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import {
	UmbEntityDetailWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';

export class UmbTemplateWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbTemplateDetailModel, UmbTemplateDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly itemRepository = new UmbTemplateItemRepository(this);

	#masterTemplate = new UmbObjectState<UmbTemplateItemModel | null>(null);
	masterTemplate = this.#masterTemplate.asObservable();

	public readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);
	public readonly alias = this._data.createObservablePartOfCurrent((data) => data?.alias);
	public readonly content = this._data.createObservablePartOfCurrent((data) => data?.content);
	public readonly masterTemplateUnique = this._data.createObservablePartOfCurrent(
		(data) => data?.masterTemplate?.unique,
	);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_TEMPLATE_WORKSPACE_ALIAS,
			entityType: UMB_TEMPLATE_ENTITY_TYPE,
			detailRepositoryAlias: UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbTemplateWorkspaceEditorElement,
				setup: async (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					await this.create({ entityType: parentEntityType, unique: parentUnique });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbTemplateWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo): void => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override async load(unique: string) {
		const response = await super.load(unique);
		if (response.data) {
			this.setMasterTemplate(response.data.masterTemplate?.unique ?? null);
		}
		return response;
	}

	async create(parent: any) {
		const data = await this.createScaffold({ parent });

		if (data) {
			if (!parent) return;
			await this.setMasterTemplate(parent.unique);
		}
		return data;
	}

	setName(value: string) {
		this._data.updateCurrent({ name: value });
	}

	setAlias(value: string) {
		this._data.updateCurrent({ alias: value });
	}

	setContent(value: string) {
		this._data.updateCurrent({ content: value });
	}

	getLayoutBlockRegexPattern() {
		return new RegExp('(@{[\\s\\S][^if]*?Layout\\s*?=\\s*?)("[^"]*?"|null)(;[\\s\\S]*?})', 'gi');
	}

	getHasLayoutBlock() {
		return this.getData()?.content ? this.getLayoutBlockRegexPattern().test(this.getData()?.content as string) : false;
	}

	async setMasterTemplate(id: string | null) {
		if (id === null) {
			this.#masterTemplate.setValue(null);
			this.#updateMasterTemplateLayoutBlock();
			return null;
		}

		const { data } = await this.itemRepository.requestItems([id]);
		if (data) {
			this.#masterTemplate.setValue(data[0]);
			this.#updateMasterTemplateLayoutBlock();
			return data[0];
		}
		return null;
	}

	#updateMasterTemplateLayoutBlock = () => {
		const currentContent = this._data.getCurrent()?.content;
		const newMasterTemplateAlias = this.#masterTemplate?.getValue()?.alias;
		const hasLayoutBlock = this.getHasLayoutBlock();

		if (this.#masterTemplate.getValue() === null && hasLayoutBlock && currentContent) {
			const newString = currentContent.replace(this.getLayoutBlockRegexPattern(), `$1null$3`);
			this.setContent(newString);
			return;
		}

		//if has layout block in the content
		if (hasLayoutBlock && currentContent) {
			const string = currentContent.replace(
				this.getLayoutBlockRegexPattern(),
				`$1"${newMasterTemplateAlias}.cshtml"$3`,
			);
			this.setContent(string);
			return;
		}

		//if no layout block in the content insert it at the beginning
		const string = `@{
	Layout = "${newMasterTemplateAlias}.cshtml";
}
${currentContent}`;
		this.setContent(string);
	};
}

export { UmbTemplateWorkspaceContext as api };
