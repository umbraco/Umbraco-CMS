import type { UmbTemplateDetailModel } from '../types.js';
import type { UmbTemplateItemModel } from '../repository/index.js';
import { UmbTemplateDetailRepository, UmbTemplateItemRepository } from '../repository/index.js';
import { UMB_TEMPLATE_WORKSPACE_ALIAS } from './manifests.js';
import { UmbTemplateWorkspaceEditorElement } from './template-workspace-editor.element.js';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import type { UmbRoutableWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceRouteManager,
} from '@umbraco-cms/backoffice/workspace';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';

export class UmbTemplateWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<UmbTemplateDetailModel>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly detailRepository = new UmbTemplateDetailRepository(this);
	public readonly itemRepository = new UmbTemplateItemRepository(this);

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	#data = new UmbObjectState<UmbTemplateDetailModel | undefined>(undefined);
	data = this.#data.asObservable();
	#masterTemplate = new UmbObjectState<UmbTemplateItemModel | null>(null);
	masterTemplate = this.#masterTemplate.asObservable();
	name = this.#data.asObservablePart((data) => data?.name);
	alias = this.#data.asObservablePart((data) => data?.alias);
	content = this.#data.asObservablePart((data) => data?.content);
	readonly unique = this.#data.asObservablePart((data) => data?.unique);
	readonly entityType = this.#data.asObservablePart((data) => data?.entityType);
	masterTemplateUnique = this.#data.asObservablePart((data) => data?.masterTemplate?.unique);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPLATE_WORKSPACE_ALIAS);
		this.#loadCodeEditor();

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbTemplateWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					this.create({ entityType: parentEntityType, unique: parentUnique });

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
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	protected override resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	async #loadCodeEditor() {
		try {
			await loadCodeEditor();
			this.#isCodeEditorReady.setValue(true);
		} catch (error) {
			console.error(error);
		}
	}

	getEntityType(): string {
		return 'template';
	}

	getUnique() {
		return this.getData()?.unique;
	}

	getData() {
		return this.#data.getValue();
	}

	setName(value: string) {
		this.#data.update({ name: value });
	}

	setAlias(value: string) {
		this.#data.update({ alias: value });
	}

	setContent(value: string) {
		this.#data.update({ content: value });
	}

	getLayoutBlockRegexPattern() {
		return new RegExp('(@{[\\s\\S][^if]*?Layout\\s*?=\\s*?)("[^"]*?"|null)(;[\\s\\S]*?})', 'gi');
	}

	getHasLayoutBlock() {
		return this.getData()?.content ? this.getLayoutBlockRegexPattern().test(this.getData()?.content as string) : false;
	}

	async load(unique: string) {
		this.resetState();
		const { data } = await this.detailRepository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.setMasterTemplate(data.masterTemplate?.unique ?? null);
			this.#data.setValue(data);
		}
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
		const currentContent = this.#data.getValue()?.content;
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

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent.setValue(parent);
		const { data } = await this.detailRepository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.setValue(data);

		if (!parent) return;
		await this.setMasterTemplate(parent.unique);
	}

	async submit() {
		if (!this.#data.value) throw new Error('Data is missing');

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');
			const { error, data } = await this.detailRepository.create(this.#data.value, parent.unique);
			if (error) throw new Error(error.message);
			this.#data.setValue(data);
			this.setIsNew(false);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
		} else {
			const { error, data } = await this.detailRepository.save(this.#data.value);
			if (error) throw new Error(error.message);
			this.#data.setValue(data);

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}

	public override destroy() {
		this.#data.destroy();
		super.destroy();
	}
}
export { UmbTemplateWorkspaceContext as api };
