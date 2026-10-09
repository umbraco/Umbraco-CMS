import { UMB_RELATION_TYPE_ENTITY_TYPE } from '../entity.js';
import { UmbRelationTypeMenuStructureWorkspaceContext } from './relation-type-menu-structure.context.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbStructureItemModel } from '@umbraco-cms/backoffice/menu';
import { UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';

class UmbRelationTypeWorkspaceContextStub extends UmbContextBase {
	readonly #unique = new UmbBasicState<string | undefined>(undefined);
	readonly unique = this.#unique.asObservable();

	// Fixed for the lifetime of the context, just like the real workspace context: it's what
	// UMB_RELATION_TYPE_WORKSPACE_CONTEXT's apiCheck discriminates on, evaluated once when
	// consumeContext resolves, so it must already be correct by then rather than being set
	// reactively later.
	readonly #entityType = new UmbBasicState<string | undefined>(UMB_RELATION_TYPE_ENTITY_TYPE);
	readonly entityType = this.#entityType.asObservable();

	readonly #name = new UmbBasicState<string | undefined>(undefined);
	readonly name = this.#name.asObservable();

	constructor(host: UmbControllerHost) {
		// Discriminated by `getEntityType() === 'relation-type'`, see
		// UMB_RELATION_TYPE_WORKSPACE_CONTEXT's apiCheck.
		super(host, 'UmbWorkspaceContext');
	}

	getEntityType() {
		return this.#entityType.getValue();
	}

	setUnique(unique: string | undefined) {
		this.#unique.setValue(unique);
	}

	setName(name: string | undefined) {
		this.#name.setValue(name);
	}
}

@customElement('umb-test-relation-type-menu-structure-host')
class UmbTestRelationTypeMenuStructureHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	workspaceContext!: UmbRelationTypeWorkspaceContextStub;

	override connectedCallback() {
		super.connectedCallback();
		this.workspaceContext = new UmbRelationTypeWorkspaceContextStub(this);
	}
}

const ROOT_ITEM: UmbStructureItemModel = {
	unique: null,
	entityType: 'relations-root',
	name: '#treeHeaders_relations',
	isFolder: false,
};

async function flushMicrotasks() {
	// Two ticks: one for context-consumer resolution, one for the inner observe to fire.
	await Promise.resolve();
	await Promise.resolve();
	await new Promise((resolve) => setTimeout(resolve, 0));
}

describe('UmbRelationTypeMenuStructureWorkspaceContext', () => {
	let host: UmbTestRelationTypeMenuStructureHostElement;
	let context: UmbRelationTypeMenuStructureWorkspaceContext;

	beforeEach(async () => {
		host = await fixture(
			html`<umb-test-relation-type-menu-structure-host></umb-test-relation-type-menu-structure-host>`,
		);
		context = new UmbRelationTypeMenuStructureWorkspaceContext(host as unknown as UmbControllerHost);
		await flushMicrotasks();
	});

	afterEach(() => {
		context.destroy();
		host.remove();
	});

	it('includes the root and the current item for an existing entity', async () => {
		host.workspaceContext.setUnique('test-unique');
		host.workspaceContext.setName('Related Documents');
		await flushMicrotasks();

		expect(await firstValueFrom(context.structure)).to.deep.equal([
			ROOT_ITEM,
			{
				unique: 'test-unique',
				entityType: UMB_RELATION_TYPE_ENTITY_TYPE,
				name: 'Related Documents',
				isFolder: false,
			},
		]);
	});

	it('includes both the root and the current item even before the entity has a unique (unlike the other flat-list menu contexts, this one does not special-case new entities)', async () => {
		host.workspaceContext.setUnique(undefined);
		host.workspaceContext.setName('New Relation Type');
		await flushMicrotasks();

		expect(await firstValueFrom(context.structure)).to.deep.equal([
			ROOT_ITEM,
			{
				unique: null,
				entityType: UMB_RELATION_TYPE_ENTITY_TYPE,
				name: 'New Relation Type',
				isFolder: false,
			},
		]);
	});

	describe('UMB_PARENT_ENTITY_CONTEXT', () => {
		it('is the root for an existing entity', async () => {
			host.workspaceContext.setUnique('test-unique');
			host.workspaceContext.setName('Related Documents');
			await flushMicrotasks();

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({ unique: ROOT_ITEM.unique, entityType: ROOT_ITEM.entityType });
		});

		it('is the root before the entity has a unique', async () => {
			host.workspaceContext.setUnique(undefined);
			host.workspaceContext.setName('New Relation Type');
			await flushMicrotasks();

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({ unique: ROOT_ITEM.unique, entityType: ROOT_ITEM.entityType });
		});
	});
});
