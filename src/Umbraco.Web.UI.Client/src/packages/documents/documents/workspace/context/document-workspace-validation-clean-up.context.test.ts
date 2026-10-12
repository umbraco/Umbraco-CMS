import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UMB_VALIDATION_CONTEXT, type UmbValidationController } from '@umbraco-cms/backoffice/validation';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const VARIANT_DOCUMENT_TYPE_ID = 'variant-documents-variant-document-type-id';

// TODO: Import instead of local definition. [NL]
class UmbTestValidationContextConsumerController extends UmbControllerBase {}

describe('UmbContentDetailWorkspaceContextBase (property validation clean up)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;
	let validation: UmbValidationController;

	before(() => {
		umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestDocumentWorkspaceHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
		context = new UmbDocumentWorkspaceContext(hostElement);

		// The variant document type has two properties ('text' and 'variantText'), giving us a property to
		// remove from the structure below, while the document itself stays loaded throughout. [NL]
		await context.load(VARIANT_DOCUMENT_ID);

		const consumedValidation = await new UmbTestValidationContextConsumerController(hostElement)
			.consumeContext(UMB_VALIDATION_CONTEXT, () => {})
			.asPromise();
		if (!consumedValidation) throw new Error('Could not resolve the Validation Context of the workspace.');
		validation = consumedValidation;
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	function variantTextPropertyUnique(): string {
		const unique = context.structure.getOwnerContentType()?.properties.find((p) => p.alias === 'variantText')?.unique;
		if (!unique) throw new Error("Could not find the 'variantText' property on the mock Content Type.");
		return unique;
	}

	it('does not remove a property message on the first content-type structure emission (baseline only)', () => {
		validation.messages.addMessage('server', "$.values[?(@.alias == 'variantText')].value", 'error');

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('removes the message of a property removed from the content type structure (e.g. a composition edited while the document is open)', async () => {
		validation.messages.addMessage('server', "$.values[?(@.alias == 'text')].value", 'error-text');
		validation.messages.addMessage('server', "$.values[?(@.alias == 'variantText')].value", 'error-variant-text');

		await context.structure.removeProperty(VARIANT_DOCUMENT_TYPE_ID, variantTextPropertyUnique());

		expect(validation.messages.getMessages()?.length).to.equal(1);
		expect(validation.messages.getMessages()?.[0].body).to.equal('error-text');
	});

	it('does not remove the message of a property that is still part of the content type structure', async () => {
		validation.messages.addMessage('server', "$.values[?(@.alias == 'text')].value", 'error-text');

		await context.structure.removeProperty(VARIANT_DOCUMENT_TYPE_ID, variantTextPropertyUnique());

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});
});
