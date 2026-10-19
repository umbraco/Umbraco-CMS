import { UmbBlockManagerContext } from './block-manager.context.js';
import type { UmbBlockDataModel } from '../types.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementDetailRepository, type UmbElementDetailModel } from '@umbraco-cms/backoffice/element';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbApiError } from '@umbraco-cms/backoffice/resources';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_VALIDATION_CONTEXT, UmbValidationContext } from '@umbraco-cms/backoffice/validation';

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
	});
});
