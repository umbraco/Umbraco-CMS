import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import {
	UMB_VALIDATION_CONTEXT,
	UmbDataPathVariantQuery,
	type UmbValidationController,
} from '@umbraco-cms/backoffice/validation';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const EN_US = new UmbVariantId('en-US');
const EN_US_NAME_PATH = `$.variants[${UmbDataPathVariantQuery(EN_US)}].name`;

class UmbTestValidationContextConsumerController extends UmbControllerBase {}

describe('UmbContentDetailWorkspaceContextBase (missing variant name)', () => {
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

	async function runMandatoryValidation(): Promise<boolean> {
		const saveData = await context.constructSaveData([EN_US]);
		return context.runMandatoryValidationForSaveData(saveData, [EN_US]).then(
			() => true,
			() => false,
		);
	}

	function nameMessages() {
		return validation.messages.getMessages().filter((x) => x.path === EN_US_NAME_PATH);
	}

	it('reports a missing name with a message type that is not owned by the form control', async () => {
		context.setName('', EN_US);

		expect(await runMandatoryValidation()).to.be.false;

		const messages = nameMessages();
		expect(messages.length).to.equal(1);
		expect(messages[0].type).to.not.equal('client');
	});

	it('removes the missing name message once the variant is given a name', async () => {
		context.setName('', EN_US);
		await runMandatoryValidation();

		context.setName('Named', EN_US);

		expect(nameMessages().length).to.equal(0);
	});

	it('passes publish validation once a name is entered after a save failed for the missing name', async () => {
		context.setName('', EN_US);
		const saved = await context.requestSave().then(
			() => true,
			() => false,
		);
		expect(saved).to.be.false;

		context.setName('Named', EN_US);
		let valid: boolean | undefined;
		await context.validateVariantsAndSubmit(
			[EN_US],
			async () => {
				valid = true;
			},
			async () => {
				valid = false;
			},
		);

		expect(valid).to.be.true;
	});
});
