import { UmbBlockManagerContext } from './block-manager.context.js';
import type { UmbBlockDataObjectModel } from './block-manager.context.js';
import type { UmbBlockWorkspaceOriginData } from '../workspace/index.js';
import type { UmbBlockDataModel, UmbBlockLayoutBaseModel } from '../types.js';
import { UmbElementDetailStore } from '../../../elements/repository/detail/element-detail.store.js';
import { aTimeout, expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbApiError } from '@umbraco-cms/backoffice/resources';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_VALIDATION_CONTEXT, UmbValidationContext } from '@umbraco-cms/backoffice/validation';
import { filter, firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UMB_ACTION_EVENT_CONTEXT, UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';
import {
	UmbElementDetailRepository,
	UMB_ELEMENT_ENTITY_TYPE,
	type UmbElementDetailModel,
} from '@umbraco-cms/backoffice/element';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

@customElement('umb-test-block-manager-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestBlockManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * A minimal, concrete stand-in for `UmbBlockManagerContext` — only enough to construct it and drive its
 * `contents`/`settings` state via the public API. `createWithPresets`/`insert` are not exercised by these tests.
 */
class UmbTestBlockManagerContext extends UmbBlockManagerContext {
	createWithPresets(): Promise<undefined> {
		throw new Error('Not used in this test.');
	}
	insert(): boolean {
		throw new Error('Not used in this test.');
	}
}

const contentDataPath = (key: string) => `$.contentData[?(@.key == '${key}')]`;
const settingsDataPath = (key: string) => `$.settingsData[?(@.key == '${key}')]`;
const blockData = (key: string): UmbBlockDataModel => ({ key, contentTypeKey: 'test-content-type', values: [] });

describe('UmbBlockManagerContext', () => {
	describe('validation clean up', () => {
		let host: UmbControllerHostElement;
		let validation: UmbValidationContext;
		let manager: UmbTestBlockManagerContext;

		beforeEach(async () => {
			host = await fixture(html`<umb-test-block-manager-host></umb-test-block-manager-host>`);
			validation = new UmbValidationContext(host);

			manager = new UmbTestBlockManagerContext(host);
			// Confirm the Validation Context is actually resolvable via consumeContext before relying on the
			// clean up wiring below — that wiring resolves the same context the same way, internally. [NL]
			await manager.consumeContext(UMB_VALIDATION_CONTEXT, () => {}).asPromise();
		});

		afterEach(() => {
			manager.destroy();
			validation.destroy();
		});

		it('does not remove a content message on the first emission (baseline only)', () => {
			validation.messages.addMessage(
				'server',
				`${contentDataPath('a')}.values[?(@.alias == 'title')].value`,
				'error-a',
			);

			manager.setContents([blockData('a')]);

			expect(validation.messages.getHasAnyMessages()).to.be.true;
		});

		it('removes the content message of a Block removed from the layout', () => {
			validation.messages.addMessage(
				'server',
				`${contentDataPath('a')}.values[?(@.alias == 'title')].value`,
				'error-a',
			);
			validation.messages.addMessage(
				'server',
				`${contentDataPath('b')}.values[?(@.alias == 'title')].value`,
				'error-b',
			);
			manager.setContents([blockData('a'), blockData('b')]);

			manager.removeOneContent('a');

			expect(validation.messages.getMessages()?.length).to.equal(1);
			expect(validation.messages.getMessages()?.[0].body).to.equal('error-b');
		});

		it('does not remove a content message when an unrelated Block is removed', () => {
			validation.messages.addMessage(
				'server',
				`${contentDataPath('a')}.values[?(@.alias == 'title')].value`,
				'error-a',
			);
			manager.setContents([blockData('a'), blockData('b')]);

			manager.removeOneContent('b');

			expect(validation.messages.getHasAnyMessages()).to.be.true;
		});

		it('removes the settings message of a Block whose settings are removed', () => {
			validation.messages.addMessage(
				'server',
				`${settingsDataPath('a')}.values[?(@.alias == 'color')].value`,
				'error-a-settings',
			);
			manager.setSettings([blockData('a')]);

			manager.removeOneSettings('a');

			expect(validation.messages.getHasAnyMessages()).to.be.false;
		});

		it('does not remove a settings message when the corresponding content Block is removed instead', () => {
			// Content and settings are cleaned up against their own scope path ($.contentData vs $.settingsData),
			// removing one must not affect messages that live under the other. [NL]
			validation.messages.addMessage(
				'server',
				`${settingsDataPath('a')}.values[?(@.alias == 'color')].value`,
				'error-a-settings',
			);
			manager.setContents([blockData('a')]);
			manager.setSettings([blockData('a')]);

			manager.removeOneContent('a');

			expect(validation.messages.getHasAnyMessages()).to.be.true;
		});

		it('does not clean up messages of a Validation Context that is not its own (a different host)', async () => {
			const parentHost = await fixture<UmbControllerHostElement>(
				html`<umb-test-block-manager-host></umb-test-block-manager-host>`,
			);
			const foreignValidation = new UmbValidationContext(parentHost);

			const childHost = document.createElement('umb-test-block-manager-host') as UmbControllerHostElement;
			parentHost.appendChild(childHost);
			const foreignManager = new UmbTestBlockManagerContext(childHost);
			await foreignManager.consumeContext(UMB_VALIDATION_CONTEXT, () => {}).asPromise();

			foreignValidation.messages.addMessage(
				'server',
				`${contentDataPath('a')}.values[?(@.alias == 'title')].value`,
				'error-a',
			);
			foreignManager.setContents([blockData('a')]);

			foreignManager.removeOneContent('a');

			expect(foreignValidation.messages.getHasAnyMessages(), 'a foreign Validation Context must be left alone').to.be
				.true;

			foreignManager.destroy();
			foreignValidation.destroy();
		});
	});
	describe('external content', () => {
		let host: UmbControllerHostElement;
		let manager: UmbTestBlockManagerContext;
		let requestedBatches: Array<Array<string>>;
		const originalRequestByUniques = UmbElementDetailRepository.prototype.requestByUniques;

		const layout = (key: string, contentKey: string) => ({ key, contentKey, isExternalContent: true });

		beforeEach(async () => {
			requestedBatches = [];
			UmbElementDetailRepository.prototype.requestByUniques = async function (uniques: Array<string>) {
				requestedBatches.push(uniques);
				return {
					data: uniques.map((unique) => ({
						unique,
						documentType: { unique: 'test-element-type' },
						values: [],
						variants: [],
					})) as unknown as Array<UmbElementDetailModel>,
				};
			};

			host = await fixture(html`<umb-test-block-manager-host></umb-test-block-manager-host>`);
			manager = new UmbTestBlockManagerContext(host);
		});

		afterEach(() => {
			UmbElementDetailRepository.prototype.requestByUniques = originalRequestByUniques;
			manager.destroy();
		});

		const hasContent = (key: string) => {
			let found = false;
			const observer = manager.observe(manager.contentOf(key), (content) => (found = !!content), null);
			observer.destroy();
			return found;
		};

		it('requests each referenced element once in a single batch, even when several blocks reference it', async () => {
			manager.setLayouts([layout('l1', 'e1'), layout('l2', 'e1'), layout('l3', 'e2')]);

			await waitUntil(() => hasContent('e1') && hasContent('e2'));

			expect(requestedBatches.flat().sort()).to.deep.equal(['e1', 'e2']);
			expect(requestedBatches).to.have.length(1);
		});

		it('only requests the content referenced by external layouts', async () => {
			manager.setLayouts([layout('l1', 'e1'), { key: 'l2', contentKey: 'local-content' }]);

			await waitUntil(() => hasContent('e1'));

			expect(requestedBatches).to.deep.equal([['e1']]);
		});

		it('drops the external content once no layout references it anymore', async () => {
			manager.setLayouts([layout('l1', 'e1'), layout('l2', 'e2')]);
			await waitUntil(() => hasContent('e1') && hasContent('e2'));

			manager.setLayouts([layout('l2', 'e2')]);

			await waitUntil(() => !hasContent('e1'));
			expect(hasContent('e2')).to.be.true;
		});
	});

	describe('expose ordering', () => {
		it('keeps exposes sorted by culture, segment and content key regardless of insertion order', async () => {
			const host = await fixture<UmbControllerHostElement>(
				html`<umb-test-block-manager-host></umb-test-block-manager-host>`,
			);
			const manager = new UmbTestBlockManagerContext(host);

			manager.setOneExpose('b', new UmbVariantId('en-us'));
			manager.setOneExpose('a', new UmbVariantId('en-us'));
			manager.setOneExpose('b', new UmbVariantId(null));

			expect(manager.getExposes().map((x) => `${x.culture}|${x.contentKey}`)).to.deep.equal([
				'null|b',
				'en-us|a',
				'en-us|b',
			]);

			manager.destroy();
		});
	});

	describe('external content failures', () => {
		const originalRequestByUnique = UmbElementDetailRepository.prototype.requestByUnique;
		const originalRequestByUniques = UmbElementDetailRepository.prototype.requestByUniques;
		let requestedKeys: Array<string>;
		let respondWithStatus: number;
		let manager: UmbTestBlockManagerContext;

		const externalLayout = (key: string) => ({ key: `layout-${key}`, contentKey: key, isExternalContent: true });
		const settle = () => new Promise((resolve) => setTimeout(resolve, 20));
		const failureOf = (key: string) => {
			let reason: string | undefined;
			manager.observe(manager.externalContentFailureOf(key), (value) => (reason = value), null);
			return reason;
		};

		beforeEach(async () => {
			requestedKeys = [];
			respondWithStatus = 404;
			// The batched request does not return the missing elements, so each one is requested on its own to find out why.
			UmbElementDetailRepository.prototype.requestByUniques = async () => ({ data: [] });
			UmbElementDetailRepository.prototype.requestByUnique = async function (unique: string) {
				requestedKeys.push(unique);
				return {
					error: new UmbApiError('error', respondWithStatus, undefined, {
						title: 'error',
						type: 'ApiError',
						status: respondWithStatus,
					}),
				};
			} as typeof originalRequestByUnique;

			const host = await fixture<UmbControllerHostElement>(
				html`<umb-test-block-manager-host></umb-test-block-manager-host>`,
			);
			manager = new UmbTestBlockManagerContext(host);
		});

		afterEach(() => {
			UmbElementDetailRepository.prototype.requestByUnique = originalRequestByUnique;
			UmbElementDetailRepository.prototype.requestByUniques = originalRequestByUniques;
			manager.destroy();
		});

		it('records a missing element as deleted', async () => {
			manager.setLayouts([externalLayout('a')]);

			await waitUntil(() => failureOf('a') === 'deleted');
		});

		it('records a forbidden element as no access', async () => {
			respondWithStatus = 403;
			manager.setLayouts([externalLayout('a')]);

			await waitUntil(() => failureOf('a') === 'noAccess');
		});

		it('does not request an element again once it is known to be unavailable', async () => {
			manager.setLayouts([externalLayout('a')]);
			await waitUntil(() => failureOf('a') === 'deleted');
			manager.setLayouts([externalLayout('a'), externalLayout('b')]);
			await waitUntil(() => failureOf('b') === 'deleted');

			expect(requestedKeys).to.deep.equal(['a', 'b']);
		});

		it('does not record a failure that says nothing about availability', async () => {
			respondWithStatus = 500;
			manager.setLayouts([externalLayout('a')]);
			await waitUntil(() => requestedKeys.includes('a'));
			await settle();

			expect(failureOf('a')).to.equal(undefined);
		});

		it('asks about one element only when a failure says nothing about availability', async () => {
			respondWithStatus = 500;
			manager.setLayouts([externalLayout('a'), externalLayout('b'), externalLayout('c')]);
			await waitUntil(() => requestedKeys.includes('a'));
			await settle();

			expect(requestedKeys).to.deep.equal(['a']);
		});

		it('asks again once the statuses change after a failure that said nothing about availability', async () => {
			respondWithStatus = 500;
			manager.setLayouts([externalLayout('a')]);
			await waitUntil(() => requestedKeys.includes('a'));
			await settle();

			respondWithStatus = 404;
			manager.setLayouts([externalLayout('a'), externalLayout('b')]);

			await waitUntil(() => failureOf('a') === 'deleted');
		});
	});
});

const ELEMENT_UNIQUE = 'library-element-one-id';
const ELEMENT_NAME = 'Element One (Library)';

class UmbTestExternalContentBlockManagerContext extends UmbBlockManagerContext<
	UmbBlockTypeBaseModel,
	UmbBlockLayoutBaseModel,
	UmbBlockWorkspaceOriginData
> {
	async createWithPresets(): Promise<UmbBlockDataObjectModel<UmbBlockLayoutBaseModel> | undefined> {
		return undefined;
	}
	insert(): boolean {
		return true;
	}
}

@customElement('umb-test-block-manager-context-host')
class UmbTestBlockManagerContextHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	actionEventContext = new UmbActionEventContext(this);
	constructor() {
		super();
		new UmbElementDetailStore(this);
	}
}

describe('UmbBlockManagerContext - external content (Library Elements)', () => {
	let host: UmbTestBlockManagerContextHostElement;
	let manager: UmbTestExternalContentBlockManagerContext;

	before(async () => {
		await useMockSet('blocksReusableContent');
	});

	beforeEach(() => {
		host = new UmbTestBlockManagerContextHostElement();
		document.body.appendChild(host);
		manager = new UmbTestExternalContentBlockManagerContext(host);
	});

	afterEach(() => {
		manager.destroy();
		document.body.innerHTML = '';
	});

	it('resolves the referenced Element into content and name', async () => {
		manager.insertExternalContent(ELEMENT_UNIQUE);

		const content = await firstValueFrom(manager.contentOf(ELEMENT_UNIQUE).pipe(filter((value) => value !== undefined)));
		expect(content!.values.find((value) => value.alias === 'title')?.value).to.equal('Reusable Element One');

		const name = await firstValueFrom(manager.externalContentNameOf(ELEMENT_UNIQUE).pipe(filter((value) => value !== null)));
		expect(name).to.equal(ELEMENT_NAME);
	});

	it('updates the block content and name when the Element is saved elsewhere in the session', async () => {
		manager.insertExternalContent(ELEMENT_UNIQUE);
		await firstValueFrom(manager.contentOf(ELEMENT_UNIQUE).pipe(filter((value) => value !== undefined)));

		// Simulate the Element workspace saving the Element (data source write) and, like the real
		// workspace does on save/publish, notifying the rest of the session that it changed.
		const repository = new UmbElementDetailRepository(host);
		const { data: element } = await repository.requestByUnique(ELEMENT_UNIQUE);
		const updated: UmbElementDetailModel = {
			...element!,
			variants: element!.variants.map((variant) => ({ ...variant, name: 'Renamed Element' })),
			values: element!.values.map((value) =>
				value.alias === 'title' ? { ...value, value: 'Updated title' } : value,
			),
		};
		await repository.save(updated);

		const eventContext = await manager.getContext(UMB_ACTION_EVENT_CONTEXT);
		eventContext!.dispatchEvent(
			new UmbEntityUpdatedEvent({ unique: ELEMENT_UNIQUE, entityType: UMB_ELEMENT_ENTITY_TYPE }),
		);

		const name = await firstValueFrom(
			manager.externalContentNameOf(ELEMENT_UNIQUE).pipe(filter((value) => value === 'Renamed Element')),
		);
		expect(name).to.equal('Renamed Element');

		const content = await firstValueFrom(
			manager
				.contentOf(ELEMENT_UNIQUE)
				.pipe(filter((value) => value?.values.find((v) => v.alias === 'title')?.value === 'Updated title')),
		);
		expect(content!.values.find((value) => value.alias === 'title')?.value).to.equal('Updated title');
	});

	it('drops the resolved content when the layout no longer references external content', async () => {
		manager.insertExternalContent(ELEMENT_UNIQUE);
		await firstValueFrom(manager.contentOf(ELEMENT_UNIQUE).pipe(filter((value) => value !== undefined)));

		manager.setLayouts([]);

		await firstValueFrom(manager.contentOf(ELEMENT_UNIQUE).pipe(filter((value) => value === undefined)));
		// Give any late-arriving fetch a chance to (wrongly) re-add the entry before asserting it stays gone.
		await aTimeout(50);
		expect(manager.getContentOf(ELEMENT_UNIQUE)).to.be.undefined;
	});

	describe('requests', () => {
		const originalRequestByUniques = UmbElementDetailRepository.prototype.requestByUniques;
		let requestCount: number;
		let failFirstRequest: boolean;

		beforeEach(() => {
			requestCount = 0;
			failFirstRequest = false;
			UmbElementDetailRepository.prototype.requestByUniques = function (this: UmbElementDetailRepository, uniques) {
				requestCount++;
				if (failFirstRequest && requestCount === 1) {
					return Promise.resolve({ error: new Error('Request failed') }) as never;
				}
				return originalRequestByUniques!.call(this, uniques);
			};
		});

		afterEach(() => {
			UmbElementDetailRepository.prototype.requestByUniques = originalRequestByUniques;
		});

		it('requests an Element again when its reference is removed and re-added after a failed request', async () => {
			failFirstRequest = true;
			manager.insertExternalContent(ELEMENT_UNIQUE);
			await aTimeout(50);
			expect(manager.getContentOf(ELEMENT_UNIQUE)).to.be.undefined;

			manager.setLayouts([]);
			manager.insertExternalContent(ELEMENT_UNIQUE);

			const content = await firstValueFrom(manager.contentOf(ELEMENT_UNIQUE).pipe(filter((value) => value !== undefined)));
			expect(content!.key).to.equal(ELEMENT_UNIQUE);
			expect(requestCount).to.equal(2);
		});
	});
});
