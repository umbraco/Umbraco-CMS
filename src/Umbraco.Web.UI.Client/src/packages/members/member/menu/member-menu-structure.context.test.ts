import { UMB_MEMBER_ENTITY_TYPE, UMB_MEMBER_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberMenuStructureWorkspaceContext } from './member-menu-structure.context.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbVariantStructureItemModel } from '@umbraco-cms/backoffice/menu';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

class UmbMemberWorkspaceContextStub extends UmbContextBase {
	readonly #unique = new UmbBasicState<string | null | undefined>(undefined);
	readonly unique = this.#unique.asObservable();

	// Fixed for the lifetime of the context, just like the real workspace context: it's what
	// UMB_MEMBER_WORKSPACE_CONTEXT's apiCheck discriminates on, evaluated once when consumeContext
	// resolves, so it must already be correct by then rather than being set reactively later.
	readonly #entityType = new UmbBasicState<string | undefined>(UMB_MEMBER_ENTITY_TYPE);
	readonly entityType = this.#entityType.asObservable();

	readonly #variants = new UmbBasicState<Array<UmbEntityVariantModel>>([]);
	readonly variants = this.#variants.asObservable();

	readonly #isNew = new UmbBasicState<boolean | undefined>(undefined);
	readonly isNew = this.#isNew.asObservable();

	constructor(host: UmbControllerHost) {
		// Discriminated by `getEntityType() === UMB_MEMBER_ENTITY_TYPE`, see
		// UMB_MEMBER_WORKSPACE_CONTEXT's apiCheck.
		super(host, 'UmbWorkspaceContext');
	}

	getEntityType() {
		return this.#entityType.getValue();
	}

	setUnique(unique: string | null | undefined) {
		this.#unique.setValue(unique);
	}

	setVariants(variants: Array<UmbEntityVariantModel>) {
		this.#variants.setValue(variants);
	}

	setIsNew(isNew: boolean | undefined) {
		this.#isNew.setValue(isNew);
	}

	getIsNew() {
		return this.#isNew.getValue();
	}
}

@customElement('umb-test-member-menu-structure-host')
class UmbTestMemberMenuStructureHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	workspaceContext!: UmbMemberWorkspaceContextStub;

	override connectedCallback() {
		super.connectedCallback();
		this.workspaceContext = new UmbMemberWorkspaceContextStub(this);
	}
}

const ROOT_ITEM: UmbVariantStructureItemModel = {
	unique: null,
	entityType: UMB_MEMBER_ROOT_ENTITY_TYPE,
	variants: [{ name: '#treeHeaders_member', culture: null, segment: null }],
};

const INPUT_VARIANT: UmbEntityVariantModel = {
	name: 'John Doe',
	culture: null,
	segment: null,
	createDate: null,
	updateDate: null,
	flags: [],
};

const CURRENT_ITEM: UmbVariantStructureItemModel = {
	unique: 'test-unique',
	entityType: UMB_MEMBER_ENTITY_TYPE,
	variants: [{ name: 'John Doe', culture: null, segment: null }],
};

async function flushMicrotasks() {
	// Two ticks: one for context-consumer resolution, one for the inner observe to fire.
	await Promise.resolve();
	await Promise.resolve();
	await new Promise((resolve) => setTimeout(resolve, 0));
}

describe('UmbMemberMenuStructureWorkspaceContext', () => {
	let host: UmbTestMemberMenuStructureHostElement;
	let context: UmbMemberMenuStructureWorkspaceContext;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-member-menu-structure-host></umb-test-member-menu-structure-host>`);
		context = new UmbMemberMenuStructureWorkspaceContext(host as unknown as UmbControllerHost);
		await flushMicrotasks();
	});

	afterEach(() => {
		context.destroy();
		host.remove();
	});

	it('includes only the root item while the entity is new', async () => {
		host.workspaceContext.setUnique(null);
		host.workspaceContext.setVariants([{ ...INPUT_VARIANT, name: 'New Member' }]);
		host.workspaceContext.setIsNew(true);
		await flushMicrotasks();

		expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM]);
	});

	it('includes the root and the current item for an existing entity', async () => {
		host.workspaceContext.setUnique(CURRENT_ITEM.unique);
		host.workspaceContext.setVariants([INPUT_VARIANT]);
		host.workspaceContext.setIsNew(false);
		await flushMicrotasks();

		expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM, CURRENT_ITEM]);
	});

	it('includes the current item once a newly-created member is saved, even though isNew flips after unique and variants are set', async () => {
		host.workspaceContext.setUnique(null);
		host.workspaceContext.setVariants([{ ...INPUT_VARIANT, name: 'New Member' }]);
		host.workspaceContext.setIsNew(true);
		await flushMicrotasks();
		expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM]);

		host.workspaceContext.setUnique(CURRENT_ITEM.unique);
		host.workspaceContext.setVariants([INPUT_VARIANT]);
		host.workspaceContext.setIsNew(false);
		await flushMicrotasks();

		expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM, CURRENT_ITEM]);
	});
});
