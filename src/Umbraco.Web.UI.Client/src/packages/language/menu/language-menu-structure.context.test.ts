import { UMB_LANGUAGE_ENTITY_TYPE, UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbLanguageNavigationStructureWorkspaceContext } from './language-menu-structure.context.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbStructureItemModel } from '@umbraco-cms/backoffice/menu';
import { UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';

class UmbEntityNamedDetailWorkspaceContextStub extends UmbContextBase {
	public readonly IS_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT = true;

	readonly #unique = new UmbBasicState<string | null | undefined>(undefined);
	readonly unique = this.#unique.asObservable();

	readonly #entityType = new UmbBasicState<string | undefined>(undefined);
	readonly entityType = this.#entityType.asObservable();

	readonly #name = new UmbBasicState<string | undefined>(undefined);
	readonly name = this.#name.asObservable();

	readonly #isNew = new UmbBasicState<boolean | undefined>(undefined);
	readonly isNew = this.#isNew.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT.toString());
	}

	setUnique(unique: string | null | undefined) {
		this.#unique.setValue(unique);
	}

	setEntityType(entityType: string | undefined) {
		this.#entityType.setValue(entityType);
	}

	setName(name: string | undefined) {
		this.#name.setValue(name);
	}

	setIsNew(isNew: boolean | undefined) {
		this.#isNew.setValue(isNew);
	}

	getIsNew() {
		return this.#isNew.getValue();
	}
}

@customElement('umb-test-language-menu-structure-host')
class UmbTestLanguageMenuStructureHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	workspaceContext!: UmbEntityNamedDetailWorkspaceContextStub;

	override connectedCallback() {
		super.connectedCallback();
		this.workspaceContext = new UmbEntityNamedDetailWorkspaceContextStub(this);
	}
}

const ROOT_ITEM: UmbStructureItemModel = {
	unique: null,
	entityType: UMB_LANGUAGE_ROOT_ENTITY_TYPE,
	name: '#treeHeaders_languages',
	isFolder: false,
};

const CURRENT_ITEM: UmbStructureItemModel = {
	unique: 'test-unique',
	entityType: UMB_LANGUAGE_ENTITY_TYPE,
	name: 'Danish (Denmark)',
	isFolder: false,
};

async function flushMicrotasks() {
	// Two ticks: one for context-consumer resolution, one for the inner observe to fire.
	await Promise.resolve();
	await Promise.resolve();
	await new Promise((resolve) => setTimeout(resolve, 0));
}

describe('UmbLanguageNavigationStructureWorkspaceContext', () => {
	let host: UmbTestLanguageMenuStructureHostElement;
	let context: UmbLanguageNavigationStructureWorkspaceContext;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-language-menu-structure-host></umb-test-language-menu-structure-host>`);
		context = new UmbLanguageNavigationStructureWorkspaceContext(host as unknown as UmbControllerHost);
		await flushMicrotasks();
	});

	afterEach(() => {
		context.destroy();
		host.remove();
	});

	it('includes only the root item while the entity is new', async () => {
		host.workspaceContext.setEntityType(UMB_LANGUAGE_ENTITY_TYPE);
		host.workspaceContext.setUnique(null);
		host.workspaceContext.setName('[New Language]');
		host.workspaceContext.setIsNew(true);
		await flushMicrotasks();

		expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM]);
	});

	it('includes the root and the current item for an existing entity', async () => {
		host.workspaceContext.setEntityType(UMB_LANGUAGE_ENTITY_TYPE);
		host.workspaceContext.setUnique(CURRENT_ITEM.unique);
		host.workspaceContext.setName(CURRENT_ITEM.name);
		host.workspaceContext.setIsNew(false);
		await flushMicrotasks();

		expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM, CURRENT_ITEM]);
	});

	it('includes the current item once a newly-created language is saved, even though isNew flips after unique and name are set', async () => {
		host.workspaceContext.setEntityType(UMB_LANGUAGE_ENTITY_TYPE);
		host.workspaceContext.setUnique(null);
		host.workspaceContext.setName('[New Language]');
		host.workspaceContext.setIsNew(true);
		await flushMicrotasks();
		expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM]);

		host.workspaceContext.setUnique(CURRENT_ITEM.unique);
		host.workspaceContext.setName(CURRENT_ITEM.name);
		host.workspaceContext.setIsNew(false);
		await flushMicrotasks();

		expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM, CURRENT_ITEM]);
	});

	describe('UMB_PARENT_ENTITY_CONTEXT', () => {
		it('is the root while the entity is new', async () => {
			host.workspaceContext.setEntityType(UMB_LANGUAGE_ENTITY_TYPE);
			host.workspaceContext.setUnique(null);
			host.workspaceContext.setName('[New Language]');
			host.workspaceContext.setIsNew(true);
			await flushMicrotasks();

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({ unique: ROOT_ITEM.unique, entityType: ROOT_ITEM.entityType });
		});

		it('is the root for an existing entity', async () => {
			host.workspaceContext.setEntityType(UMB_LANGUAGE_ENTITY_TYPE);
			host.workspaceContext.setUnique(CURRENT_ITEM.unique);
			host.workspaceContext.setName(CURRENT_ITEM.name);
			host.workspaceContext.setIsNew(false);
			await flushMicrotasks();

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({ unique: ROOT_ITEM.unique, entityType: ROOT_ITEM.entityType });
		});
	});
});
