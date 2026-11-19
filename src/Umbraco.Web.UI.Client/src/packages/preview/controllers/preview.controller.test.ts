import { UmbPreviewController } from './preview.controller.js';
import type { UmbPreviewRepository } from '../repository/preview.repository.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-test-preview-controller-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestPreviewControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type OpenCall = { url: string; target?: string };

type FakeWindow = { focus: () => void; closed: boolean; focusCount: number };

function spyOnWindowOpen(reusableWindow: FakeWindow) {
	const original = window.open;
	const calls: OpenCall[] = [];
	window.open = ((url?: string | URL, target?: string) => {
		calls.push({ url: url?.toString() ?? '', target });
		return reusableWindow as unknown as WindowProxy;
	}) as typeof window.open;
	return {
		calls,
		restore: () => {
			window.open = original;
		},
	};
}

function urlInfo(overrides: Partial<DocumentUrlInfoModel> = {}): DocumentUrlInfoModel {
	return {
		message: null,
		provider: 'umbDocumentUrlProvider',
		culture: null,
		url: 'https://example.com/preview',
		isExternal: false,
		...overrides,
	};
}

class FakePreviewRepository {
	getPreviewUrlCalls: Array<{ unique: string; providerAlias: string }> = [];
	#response: DocumentUrlInfoModel;

	constructor(response: DocumentUrlInfoModel) {
		this.#response = response;
	}

	async getPreviewUrl(unique: string, providerAlias: string): Promise<DocumentUrlInfoModel> {
		this.getPreviewUrlCalls.push({ unique, providerAlias });
		return this.#response;
	}
}

const ARGS = { unique: 'doc-1', urlProviderAlias: 'umbDocumentUrlProvider' } as const;

describe('UmbPreviewController', () => {
	let host: UmbControllerHostElement;
	let fakeWindow: FakeWindow;
	let openSpy: ReturnType<typeof spyOnWindowOpen>;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-preview-controller-host></umb-test-preview-controller-host>`);
		fakeWindow = {
			closed: false,
			focusCount: 0,
			focus() {
				this.focusCount++;
			},
		};
		openSpy = spyOnWindowOpen(fakeWindow);
	});

	afterEach(() => {
		openSpy.restore();
	});

	function createController(repository: FakePreviewRepository): UmbPreviewController {
		return new UmbPreviewController(host, repository as unknown as UmbPreviewRepository);
	}

	it('opens a preview window with a cache-busting parameter and focuses it', async () => {
		const repository = new FakePreviewRepository(urlInfo());
		const controller = createController(repository);

		await controller.preview({ ...ARGS });

		expect(openSpy.calls).to.have.lengthOf(1);
		expect(openSpy.calls[0].target).to.equal('umbpreview-doc-1');
		expect(new URL(openSpy.calls[0].url).searchParams.has('rnd')).to.be.true;
		expect(fakeWindow.focusCount).to.equal(1);
	});

	describe('internal preview URL', () => {
		it('only focuses the existing window on a second preview and lets SignalR refresh it', async () => {
			const repository = new FakePreviewRepository(urlInfo({ isExternal: false }));
			const controller = createController(repository);

			await controller.preview({ ...ARGS });
			await controller.preview({ ...ARGS });

			// Second call must not open/reload a new window, nor re-request the URL.
			expect(openSpy.calls, 'window.open should only be called once').to.have.lengthOf(1);
			expect(repository.getPreviewUrlCalls, 'preview URL should be fetched only once').to.have.lengthOf(1);
			expect(fakeWindow.focusCount, 'the existing window should be focused twice').to.equal(2);
		});
	});

	describe('external preview URL', () => {
		it('re-requests a fresh URL and reloads the tab on every preview (no SignalR) — regression for #21820', async () => {
			const repository = new FakePreviewRepository(urlInfo({ isExternal: true }));
			const controller = createController(repository);

			await controller.preview({ ...ARGS });
			await controller.preview({ ...ARGS });

			// Both calls must re-fetch (fresh token) and reload the same-named tab.
			expect(repository.getPreviewUrlCalls, 'preview URL should be re-requested each time').to.have.lengthOf(2);
			expect(openSpy.calls, 'the tab should be reloaded each time').to.have.lengthOf(2);
			expect(openSpy.calls[0].target).to.equal('umbpreview-doc-1');
			expect(openSpy.calls[1].target).to.equal('umbpreview-doc-1');
			expect(fakeWindow.focusCount).to.equal(2);
		});
	});

	describe('message-only response', () => {
		it('does not open a window and throws with the message', async () => {
			const repository = new FakePreviewRepository(
				urlInfo({ url: null, message: 'No preview available' }),
			);
			const controller = createController(repository);

			let thrown: unknown;
			try {
				await controller.preview({ ...ARGS });
			} catch (error) {
				thrown = error;
			}

			expect(openSpy.calls).to.have.lengthOf(0);
			expect((thrown as Error)?.message).to.equal('No preview available');
		});
	});
});
