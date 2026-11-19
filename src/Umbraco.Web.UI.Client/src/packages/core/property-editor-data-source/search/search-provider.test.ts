import { UmbPropertyEditorDataSourceSearchProvider } from './search-provider.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-property-editor-data-source-search-provider-host')
class UmbTestSearchProviderHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbPropertyEditorDataSourceSearchProvider', () => {
	it('removes itself from its host when destroyed', () => {
		const hostElement = new UmbTestSearchProviderHostElement();
		const provider = new UmbPropertyEditorDataSourceSearchProvider(hostElement);

		provider.destroy();

		expect(hostElement.hasUmbController(provider)).to.be.false;
	});
});
