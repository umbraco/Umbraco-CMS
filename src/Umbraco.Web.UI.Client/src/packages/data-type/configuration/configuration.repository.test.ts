import { useMockHandlers, resetMockHandlers } from '../../../../mocks/index.js';
import {
	UmbDataTypesConfigurationRepository,
	resetUmbDataTypesConfigurationCache,
} from './configuration.repository.js';
import type { UmbDataTypesConfigurationModel } from './types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

const UMB_SLUG = '/data-type';

const configuration: UmbDataTypesConfigurationModel = {
	showDeprecatedPropertyEditors: true,
};

@customElement('umb-test-data-types-configuration-repository-host')
class UmbTestDataTypesConfigurationRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDataTypesConfigurationRepository', () => {
	let host: UmbTestDataTypesConfigurationRepositoryHostElement;
	let repository: UmbDataTypesConfigurationRepository;
	let requestCount: number;

	beforeEach(() => {
		requestCount = 0;
		resetUmbDataTypesConfigurationCache();
		host = new UmbTestDataTypesConfigurationRepositoryHostElement();
		document.body.appendChild(host);
		repository = new UmbDataTypesConfigurationRepository(host);
	});

	afterEach(() => {
		repository.destroy();
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	it('should map the configuration response', async () => {
		useMockHandlers(
			http.get(umbracoPath(`${UMB_SLUG}/configuration`), () => {
				requestCount++;
				return HttpResponse.json({
					canBeChanged: 'True',
					documentListViewId: 'c0808dd3-8133-4e4b-8ce8-e2bea84a96a4',
					mediaListViewId: '3a0156c4-3b8c-4803-bdc1-6871faa83fff',
					showDeprecatedPropertyEditors: true,
				});
			}),
		);

		const { data } = await repository.requestConfiguration();

		expect(requestCount).to.equal(1);
		expect(data).to.deep.equal(configuration);
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
