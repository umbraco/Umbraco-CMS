import { UmbMediaTypeSearchProvider } from './media-type.search-provider.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-media-type-search-provider-host')
class UmbTestSearchProviderHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaTypeSearchProvider', () => {
	it('removes itself from its host when destroyed', () => {
		const hostElement = new UmbTestSearchProviderHostElement();
		const provider = new UmbMediaTypeSearchProvider(hostElement);

		provider.destroy();

		expect(hostElement.hasUmbController(provider)).to.be.false;
	});
});
