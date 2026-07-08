import { useMockHandlers, resetMockHandlers } from '../../../../../mocks/index.js';
import { UmbMediaConfigurationRepository, resetUmbMediaConfigurationCache } from './configuration.repository.js';
import type { UmbMediaConfigurationModel } from './types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

const UMB_SLUG = '/media';

const configuration: UmbMediaConfigurationModel = {
	disableDeleteWhenReferenced: true,
};

@customElement('umb-test-media-configuration-repository-host')
class UmbTestMediaConfigurationRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaConfigurationRepository', () => {
	let host: UmbTestMediaConfigurationRepositoryHostElement;
	let repository: UmbMediaConfigurationRepository;
	let requestCount: number;

	beforeEach(() => {
		requestCount = 0;
		resetUmbMediaConfigurationCache();
		host = new UmbTestMediaConfigurationRepositoryHostElement();
		document.body.appendChild(host);
		repository = new UmbMediaConfigurationRepository(host);
	});

	afterEach(() => {
		repository.destroy();
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	it('should cache a successful configuration response', async () => {
		useMockHandlers(
			http.get(umbracoPath(`${UMB_SLUG}/configuration`), () => {
				requestCount++;
				return HttpResponse.json(configuration);
			}),
		);

		const first = await repository.requestConfiguration();
		const second = await repository.requestConfiguration();

		expect(requestCount).to.equal(1);
		expect(first.data).to.deep.equal(configuration);
		expect(second.data).to.deep.equal(configuration);
	});

	it('should share one request between concurrent calls', async () => {
		useMockHandlers(
			http.get(umbracoPath(`${UMB_SLUG}/configuration`), () => {
				requestCount++;
				return HttpResponse.json(configuration);
			}),
		);

		const [first, second] = await Promise.all([repository.requestConfiguration(), repository.requestConfiguration()]);

		expect(requestCount).to.equal(1);
		expect(first.data).to.deep.equal(configuration);
		expect(second.data).to.deep.equal(configuration);
	});

	it('should not cache an error response', async () => {
		let hasFailedOnce = false;
		useMockHandlers(
			http.get(umbracoPath(`${UMB_SLUG}/configuration`), () => {
				requestCount++;
				if (!hasFailedOnce) {
					hasFailedOnce = true;
					return new HttpResponse(null, { status: 500 });
				}
				return HttpResponse.json(configuration);
			}),
		);

		const first = await repository.requestConfiguration();

		expect(first.error).to.exist;
		expect(first.data).to.be.undefined;

		const second = await repository.requestConfiguration();

		expect(requestCount).to.equal(2);
		expect(second.error).to.be.undefined;
		expect(second.data).to.deep.equal(configuration);
	});
});
