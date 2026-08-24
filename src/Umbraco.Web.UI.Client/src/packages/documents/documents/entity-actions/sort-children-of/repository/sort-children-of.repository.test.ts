import { UmbSortChildrenOfDocumentRepository } from './sort-children-of.repository.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { ContentSortFieldModel, DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbDirection } from '@umbraco-cms/backoffice/utils';
import type { UmbDirectionType } from '@umbraco-cms/backoffice/utils';

@customElement('test-sort-children-of-document-repository-host')
class UmbTestSortChildrenOfDocumentRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSortChildrenOfDocumentRepository', () => {
	let hostElement: UmbTestSortChildrenOfDocumentRepositoryHostElement;
	let repository: UmbSortChildrenOfDocumentRepository;

	const original = DocumentService.putDocumentByIdSortChildren;

	let byIdOptions: any;

	beforeEach(() => {
		hostElement = new UmbTestSortChildrenOfDocumentRepositoryHostElement();
		document.body.appendChild(hostElement);
		new UmbContextProviderController(hostElement, UMB_APP_LANGUAGE_CONTEXT, {
			getAppCulture: () => 'da-DK',
			getHostElement: () => hostElement,
		} as unknown as typeof UMB_APP_LANGUAGE_CONTEXT.TYPE);
		repository = new UmbSortChildrenOfDocumentRepository(hostElement);

		byIdOptions = undefined;
		(DocumentService as any).putDocumentByIdSortChildren = (options: any) => {
			byIdOptions = options;
			return Promise.resolve({ data: undefined });
		};
	});

	afterEach(() => {
		(DocumentService as any).putDocumentByIdSortChildren = original;
		document.body.innerHTML = '';
	});

	it('sorts by the current backoffice culture when no culture is given', async () => {
		await repository.sortChildrenOfByField({
			unique: 'document-id',
			field: ContentSortFieldModel.NAME,
			direction: UmbDirection.ASCENDING as UmbDirectionType,
		});

		expect(byIdOptions?.body?.culture).to.equal('da-DK');
	});

	it('sorts by an explicitly given culture', async () => {
		await repository.sortChildrenOfByField({
			unique: 'document-id',
			field: ContentSortFieldModel.NAME,
			direction: UmbDirection.ASCENDING as UmbDirectionType,
			culture: 'en-US',
		});

		expect(byIdOptions?.body?.culture).to.equal('en-US');
	});
});
