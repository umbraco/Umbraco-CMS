import { UmbDocumentTypeSearchProvider } from './document-type.search-provider.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-document-type-search-provider-host')
class UmbTestSearchProviderHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDocumentTypeSearchProvider', () => {
	it('removes itself from its host when destroyed', () => {
		const hostElement = new UmbTestSearchProviderHostElement();
		const provider = new UmbDocumentTypeSearchProvider(hostElement);

		provider.destroy();

		expect(hostElement.hasUmbController(provider)).to.be.false;
	});
});
