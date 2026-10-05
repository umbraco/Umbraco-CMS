import { UmbSortChildrenOfDocumentRepository } from './sort-children-of.repository.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { ContentSortFieldModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('test-sort-children-of-document-repository-host')
class UmbTestSortChildrenOfDocumentRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSortChildrenOfDocumentRepository', () => {
	let hostElement: UmbTestSortChildrenOfDocumentRepositoryHostElement;
	let repository: UmbSortChildrenOfDocumentRepository;

	beforeEach(() => {
		hostElement = new UmbTestSortChildrenOfDocumentRepositoryHostElement();
		document.body.appendChild(hostElement);
		repository = new UmbSortChildrenOfDocumentRepository(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('offers only the culture-dependent fields as varying by culture', async () => {
		const options = await repository.requestSortByFieldOptions();

		expect(options.find((option) => option.value === ContentSortFieldModel.NAME)?.variesByCulture).to.be.true;
		expect(options.find((option) => option.value === ContentSortFieldModel.CREATE_DATE)?.variesByCulture).to.be
			.undefined;
		expect(options.find((option) => option.value === ContentSortFieldModel.UPDATE_DATE)?.variesByCulture).to.be
			.undefined;
	});
});
