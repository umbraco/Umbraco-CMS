import type { UmbTemplateDetailModel } from '../types.js';
import type { UmbTemplateItemModel, UmbTemplateDetailRepository } from '../repository/index.js';
import { UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS, UmbTemplateItemRepository } from '../repository/index.js';
import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import { UMB_TEMPLATE_WORKSPACE_ALIAS } from './manifests.js';
import { UmbTemplateWorkspaceEditorElement } from './template-workspace-editor.element.js';
import type { UmbRoutableWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbTemplateWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbTemplateDetailModel, UmbTemplateDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly itemRepository = new UmbTemplateItemRepository(this);

	#layoutTemplate = new UmbObjectState<UmbTemplateItemModel | null>(null);
	layoutTemplate = this.#layoutTemplate.asObservable();

	public readonly alias = this._data.createObservablePartOfCurrent((data) => data?.alias);
	public readonly content = this._data.createObservablePartOfCurrent((data) => data?.content);
	public readonly layoutTemplateUnique = this._data.createObservablePartOfCurrent(
		(data) => data?.layoutTemplate?.unique,
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

		// On load we want to set the layout template details but not update the layout block in the Razor file.
		// This is because you can still set a layout in code by setting `Layout = "_Layout.cshtml";` in the template file.
		// This gets set automatically if you create a template under a parent, but you don't have to do that, you can
		// just set  the `Layout` property in the Razor template file itself.
		// So even if there's no layout template set by there being a parent, there may still be one set in the Razor
		// code, and we shouldn't overwrite it.
		await this.setLayoutTemplate(response.data?.layoutTemplate?.unique ?? null, false);

		return response;
	}

	async create(parent: UmbEntityModel) {
		const data = await this.createScaffold({
			parent,
			preset: {
				layoutTemplate: parent.unique ? { unique: parent.unique } : null,
			},
		});

		// On create set or reset the layout template depending on whether the template is being created under a parent.
		// This is important to reset when a new template is created so the UI reflects the correct state.
		await this.setLayoutTemplate(parent.unique, true);

		return data;
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

	async setLayoutTemplate(unique: string | null, updateLayoutBlock: boolean) {
		if (unique === null) {
			this.#layoutTemplate.setValue(null);
		} else {
			// We need the whole template model if the unique id is provided
			const { data } = await this.itemRepository.requestItems([unique]);
			if (data) {
				this.#layoutTemplate.setValue(data[0]);
			}
		}

		if (updateLayoutBlock) {
			this.#updateLayoutTemplateLayoutBlock();
			this._data.updateCurrent({ layoutTemplate: unique ? { unique } : null });
		}

		return unique;
	}

	#updateLayoutTemplateLayoutBlock = () => {
		const currentContent = this._data.getCurrent()?.content;
		const newLayoutTemplateAlias = this.#layoutTemplate?.getValue()?.alias;
		const hasLayoutBlock = this.getHasLayoutBlock();

		if (this.#layoutTemplate.getValue() === null && hasLayoutBlock && currentContent) {
			const newString = currentContent.replace(this.getLayoutBlockRegexPattern(), `$1null$3`);
			this.setContent(newString);
			return;
		}

		//if has layout block in the content
		if (hasLayoutBlock && currentContent) {
			const string = currentContent.replace(
				this.getLayoutBlockRegexPattern(),
				`$1"${newLayoutTemplateAlias}.cshtml"$3`,
			);
			this.setContent(string);
			return;
		}

		//if no layout block in the content insert it at the beginning
		const string = `@{
	Layout = "${newLayoutTemplateAlias}.cshtml";
}
${currentContent}`;
		this.setContent(string);
	};
}

export { UmbTemplateWorkspaceContext as api };
