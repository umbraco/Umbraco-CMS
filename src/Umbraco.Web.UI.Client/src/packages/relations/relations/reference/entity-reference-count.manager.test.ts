import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbEntityReferenceCountManager } from './entity-reference-count.manager.js';
import type { UmbEntityReferenceRepository } from './types.js';

@customElement('umb-test-reference-count-manager-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const TEST_REPOSITORY_ALIAS = 'Umb.Test.EntityReferenceCountManager.Repository';

interface UmbTestReferenceResponse {
	total: number;
	delayMs?: number;
	error?: Error & { status?: number };
}

/**
 * Each call to `requestReferencedBy` consumes the next queued response, in call order — so a test can control
 * exactly what each of several overlapping calls resolves with (and how long each takes), regardless of which
 * `unique` it was made for.
 */
class UmbTestReferenceRepository implements UmbEntityReferenceRepository {
	static responseQueue: Array<UmbTestReferenceResponse> = [];
	static callCount = 0;

	async #nextResponse() {
		UmbTestReferenceRepository.callCount++;
		const response = UmbTestReferenceRepository.responseQueue.shift() ?? { total: 0 };
		if (response.delayMs) {
			await new Promise((resolve) => setTimeout(resolve, response.delayMs));
		}
		if (response.error) {
			return { error: response.error };
		}
		return { data: { items: [], total: response.total } };
	}

	async requestReferencedBy() {
		return this.#nextResponse();
	}

	async requestAreReferenced() {
		return { data: { items: [], total: 0 } };
	}

	destroy() {}
}

describe('UmbEntityReferenceCountManager', () => {
	let hostElement: UmbTestControllerHostElement;
	let manager: UmbEntityReferenceCountManager;

	before(() => {
		const manifest: ManifestApi<UmbTestReferenceRepository> = {
			type: 'my-test-type',
			alias: TEST_REPOSITORY_ALIAS,
			name: 'Test Entity Reference Repository',
			api: UmbTestReferenceRepository,
		};
		umbExtensionsRegistry.register(manifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		manager = new UmbEntityReferenceCountManager(hostElement, { referenceRepositoryAlias: TEST_REPOSITORY_ALIAS });
		UmbTestReferenceRepository.responseQueue = [];
		UmbTestReferenceRepository.callCount = 0;
	});

	it('has no total until a unique is set', () => {
		expect(manager.getTotal()).to.be.undefined;
	});

	it('loads the total once a unique is set', async () => {
		UmbTestReferenceRepository.responseQueue.push({ total: 3 });
		await manager.setUnique('elm-1');
		expect(manager.getTotal()).to.equal(3);
	});

	it('is a no-op when the unique is unchanged', async () => {
		UmbTestReferenceRepository.responseQueue.push({ total: 3 });
		await manager.setUnique('elm-1');
		await manager.setUnique('elm-1');
		expect(UmbTestReferenceRepository.callCount).to.equal(1);
	});

	it('reloads when asked to, without a unique change', async () => {
		UmbTestReferenceRepository.responseQueue.push({ total: 3 });
		await manager.setUnique('elm-1');

		UmbTestReferenceRepository.responseQueue.push({ total: 5 });
		await manager.reload();

		expect(manager.getTotal()).to.equal(5);
	});

	it('clears the total when the unique is cleared', async () => {
		UmbTestReferenceRepository.responseQueue.push({ total: 3 });
		await manager.setUnique('elm-1');
		await manager.setUnique(undefined);
		expect(manager.getTotal()).to.be.undefined;
	});

	describe('getTotalAsync', () => {
		it('returns the cached total without a new request once loaded', async () => {
			UmbTestReferenceRepository.responseQueue.push({ total: 3 });
			await manager.setUnique('elm-1');
			const callsBefore = UmbTestReferenceRepository.callCount;

			const total = await manager.getTotalAsync();

			expect(total).to.equal(3);
			expect(UmbTestReferenceRepository.callCount, 'no extra request').to.equal(callsBefore);
		});

		// A publish action can, in principle, fire before the workspace context's own prefetch has resolved.
		// getTotalAsync must not read that race as "zero references" — it should wait for the real answer.
		it('awaits the load when called before the initial fetch resolves', async () => {
			UmbTestReferenceRepository.responseQueue.push({ total: 4, delayMs: 20 });

			const setUniquePromise = manager.setUnique('elm-1');
			const total = await manager.getTotalAsync();

			expect(total).to.equal(4);
			await setUniquePromise;
		});
	});

	describe('reload ordering', () => {
		// If a slow reload resolves after a newer, faster one, it must not clobber the fresher result.
		it('a stale, slower reload does not overwrite a newer one', async () => {
			UmbTestReferenceRepository.responseQueue.push({ total: 0 });
			await manager.setUnique('elm-1');

			UmbTestReferenceRepository.responseQueue.push({ total: 1, delayMs: 30 }, { total: 9, delayMs: 0 });

			const staleReload = manager.reload();
			const freshReload = manager.reload();
			await Promise.all([staleReload, freshReload]);

			expect(manager.getTotal()).to.equal(9);
		});
	});

	describe('errors', () => {
		it('getTotalAsync rejects when the underlying request errors, rather than reading it as zero', async () => {
			UmbTestReferenceRepository.responseQueue.push({ total: 0, error: new Error('network error') });

			let caught: unknown;
			try {
				await manager.setUnique('elm-1');
			} catch (error) {
				caught = error;
			}

			expect(caught).to.be.instanceOf(Error);
		});
	});

	describe('clear', () => {
		// clear() bumps the reload token to invalidate any reload already in flight, but a subsequent getTotalAsync
		// call must not ride along on that now-invalidated reload (which resolves without ever setting a total) —
		// it needs to issue a fresh request of its own, or it would silently misreport the count as 0. Uses a
		// `prefetch: false` manager so setUnique itself doesn't consume a queued response, isolating the race.
		it('a reload invalidated by clear() does not block the next getTotalAsync call from issuing a fresh request', async () => {
			const lazyManager = new UmbEntityReferenceCountManager(hostElement, {
				referenceRepositoryAlias: TEST_REPOSITORY_ALIAS,
				prefetch: false,
			});
			await lazyManager.setUnique('elm-1');

			UmbTestReferenceRepository.responseQueue.push({ total: 1, delayMs: 20 });
			const staleRequest = lazyManager.getTotalAsync(); // kicks off a reload that won't resolve for 20ms

			lazyManager.clear(); // invalidate it before it resolves

			UmbTestReferenceRepository.responseQueue.push({ total: 5 });
			const total = await lazyManager.getTotalAsync();

			expect(total, 'must issue a fresh request rather than await the invalidated one').to.equal(5);

			await staleRequest;
		});
	});

	describe('prefetch: false', () => {
		beforeEach(() => {
			manager = new UmbEntityReferenceCountManager(hostElement, {
				referenceRepositoryAlias: TEST_REPOSITORY_ALIAS,
				prefetch: false,
			});
		});

		it('setUnique does not issue a request', async () => {
			await manager.setUnique('elm-1');
			expect(UmbTestReferenceRepository.callCount).to.equal(0);
			expect(manager.getTotal()).to.be.undefined;
		});

		it('getTotalAsync still resolves the real total on demand', async () => {
			await manager.setUnique('elm-1');
			UmbTestReferenceRepository.responseQueue.push({ total: 7 });

			const total = await manager.getTotalAsync();

			expect(total).to.equal(7);
			expect(UmbTestReferenceRepository.callCount).to.equal(1);
		});

		// Same race as the clear() test below, but triggered by switching unique instead: a reload for the
		// previous unique must not still be ridden along on by a getTotalAsync call for the new one.
		it('a reload invalidated by switching unique does not block the next getTotalAsync call from issuing a fresh request', async () => {
			await manager.setUnique('elm-1');

			UmbTestReferenceRepository.responseQueue.push({ total: 1, delayMs: 20 });
			const staleRequest = manager.getTotalAsync(); // kicks off a reload for elm-1 that won't resolve for 20ms

			await manager.setUnique('elm-2'); // invalidate it before it resolves

			UmbTestReferenceRepository.responseQueue.push({ total: 5 });
			const total = await manager.getTotalAsync();

			expect(total, 'must issue a fresh request rather than await the invalidated one').to.equal(5);

			await staleRequest;
		});

		describe('clear', () => {
			it('drops the cached total so the next getTotalAsync call re-fetches it', async () => {
				await manager.setUnique('elm-1');
				UmbTestReferenceRepository.responseQueue.push({ total: 1 });
				await manager.getTotalAsync();

				manager.clear();
				expect(manager.getTotal()).to.be.undefined;

				UmbTestReferenceRepository.responseQueue.push({ total: 4 });
				const total = await manager.getTotalAsync();

				expect(total).to.equal(4);
				expect(UmbTestReferenceRepository.callCount).to.equal(2);
			});
		});
	});
});
