import { UmbPreviewSessionRenewalController } from './preview-session-renewal.controller.js';
import type { UmbPreviewRepository } from '../repository/preview.repository.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-test-preview-session-renewal-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestPreviewSessionRenewalHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type PreviewUrlCall = { unique: string; providerAlias: string; culture?: string; segment?: string };

class FakePreviewRepository {
	calls: PreviewUrlCall[] = [];
	fail = false;

	async getPreviewUrl(
		unique: string,
		providerAlias: string,
		culture?: string,
		segment?: string,
	): Promise<DocumentUrlInfoModel> {
		this.calls.push({ unique, providerAlias, culture, segment });
		if (this.fail) {
			throw new Error('Forbidden');
		}

		return { message: null, provider: providerAlias, culture: null, url: '/umbraco/preview', isExternal: false };
	}
}

describe('UmbPreviewSessionRenewalController', () => {
	let host: UmbControllerHostElement;
	let repository: FakePreviewRepository;
	let controller: UmbPreviewSessionRenewalController;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-preview-session-renewal-host></umb-test-preview-session-renewal-host>`);
		repository = new FakePreviewRepository();
		controller = new UmbPreviewSessionRenewalController(host, repository as unknown as UmbPreviewRepository);
	});

	it('renews the preview session through the document URL provider for the previewed document', async () => {
		await controller.renew({ unique: 'doc-1', culture: 'da', segment: 'seg' });

		expect(repository.calls).to.deep.equal([
			{ unique: 'doc-1', providerAlias: 'umbDocumentUrlProvider', culture: 'da', segment: 'seg' },
		]);
	});

	it('does not renew without a document', async () => {
		await controller.renew({});

		expect(repository.calls).to.have.lengthOf(0);
	});

	it('swallows a failed renewal', async () => {
		repository.fail = true;

		await controller.renew({ unique: 'doc-1' });

		expect(repository.calls).to.have.lengthOf(1);
	});

	it('renews each time a back-office session is emitted, and not when there is no session', async () => {
		const session = new UmbObjectState<{ expiresAt: number } | undefined>(undefined);

		controller.observeSession(session.asObservable(), () => ({ unique: 'doc-1' }));
		expect(repository.calls, 'no session yet').to.have.lengthOf(0);

		session.setValue({ expiresAt: 1 });
		session.setValue({ expiresAt: 2 });
		session.setValue(undefined);
		await Promise.resolve();

		expect(repository.calls.map((x) => x.unique)).to.deep.equal(['doc-1', 'doc-1']);
	});
});
