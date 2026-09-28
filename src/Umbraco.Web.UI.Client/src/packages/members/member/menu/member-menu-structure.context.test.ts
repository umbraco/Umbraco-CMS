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
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_EDIT_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import { UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';

const SECTION_PATHNAME = 'member-management';

class UmbSectionContextStub extends UmbContextBase {
	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_CONTEXT);
	}

	getPathname() {
		return SECTION_PATHNAME;
	}
}

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
	sectionContext!: UmbSectionContextStub;

	override connectedCallback() {
		super.connectedCallback();
		this.workspaceContext = new UmbMemberWorkspaceContextStub(this);
		this.sectionContext = new UmbSectionContextStub(this);
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

	describe('UMB_PARENT_ENTITY_CONTEXT', () => {
		it('is the root while the entity is new', async () => {
			host.workspaceContext.setUnique(null);
			host.workspaceContext.setVariants([{ ...INPUT_VARIANT, name: 'New Member' }]);
			host.workspaceContext.setIsNew(true);
			await flushMicrotasks();

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({ unique: null, entityType: UMB_MEMBER_ROOT_ENTITY_TYPE });
		});

		it('is the root for an existing entity', async () => {
			host.workspaceContext.setUnique(CURRENT_ITEM.unique);
			host.workspaceContext.setVariants([INPUT_VARIANT]);
			host.workspaceContext.setIsNew(false);
			await flushMicrotasks();

			const parentContext = await context.getContext(UMB_PARENT_ENTITY_CONTEXT);
			expect(parentContext?.getParent()).to.deep.equal({ unique: null, entityType: UMB_MEMBER_ROOT_ENTITY_TYPE });
		});
	});

	describe('getItemHref', () => {
		it('returns the workspace edit path for an item with a unique', () => {
			const href = context.getItemHref(CURRENT_ITEM);

			expect(href).to.equal(
				UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
					sectionName: SECTION_PATHNAME,
					entityType: CURRENT_ITEM.entityType,
					unique: CURRENT_ITEM.unique!,
				}),
			);
		});

		it('returns undefined for an item without a unique', () => {
			expect(context.getItemHref(ROOT_ITEM)).to.equal(undefined);
		});
	});

	describe('destroy', () => {
		it('completes the structure observable without replaying its last value', async () => {
			host.workspaceContext.setUnique(CURRENT_ITEM.unique);
			host.workspaceContext.setVariants([INPUT_VARIANT]);
			host.workspaceContext.setIsNew(false);
			await flushMicrotasks();
			expect(await firstValueFrom(context.structure)).to.deep.equal([ROOT_ITEM, CURRENT_ITEM]);

			context.destroy();

			let didEmit = false;
			let didComplete = false;
			await new Promise<void>((resolve) => {
				context.structure.subscribe({
					next: () => (didEmit = true),
					complete: () => {
						didComplete = true;
						resolve();
					},
				});
			});

			expect(didEmit).to.equal(false);
			expect(didComplete).to.equal(true);
		});
	});
});
