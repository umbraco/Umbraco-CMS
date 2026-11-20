import { UmbDocumentBlueprintWorkspaceContext } from './document-blueprint-workspace.context.js';
import { UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentBlueprintDetailStore } from '../repository/detail/document-blueprint-detail.store.js';
import { manifests as documentBlueprintDetailRepositoryManifests } from '../repository/detail/manifests.js';
import {
	TEST_MANIFESTS,
	UmbTestDocumentWorkspaceHostElement,
} from '../../documents/workspace/context/document-workspace-context.test-utils.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import {
	UMB_VALIDATION_CONTEXT,
	UmbDataPathVariantQuery,
	type UmbValidationController,
} from '@umbraco-cms/backoffice/validation';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

const DOCUMENT_TYPE_ID = 'the-simplest-document-type-id';
const MANIFESTS = [...TEST_MANIFESTS, ...documentBlueprintDetailRepositoryManifests];

@customElement('umb-test-document-blueprint-workspace-host')
class UmbTestDocumentBlueprintWorkspaceHostElement extends UmbTestDocumentWorkspaceHostElement {
	constructor() {
		super();
		new UmbDocumentBlueprintDetailStore(this);
	}
}

class UmbTestValidationContextConsumerController extends UmbControllerBase {}

describe('UmbDocumentBlueprintWorkspaceContext (missing name)', () => {
	let hostElement: UmbTestDocumentBlueprintWorkspaceHostElement;
	let context: UmbDocumentBlueprintWorkspaceContext;
	let validation: UmbValidationController;
	// The Document Type of this test does not vary, so the blueprint has the invariant variant only.
	const variantId = UmbVariantId.CreateInvariant();

	before(() => {
		umbExtensionsRegistry.registerMany(MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('default');
		hostElement = new UmbTestDocumentBlueprintWorkspaceHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
		context = new UmbDocumentBlueprintWorkspaceContext(hostElement);
		await context.create({ entityType: UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE, unique: null }, DOCUMENT_TYPE_ID);

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
		const saveData = await context.constructSaveData([variantId]);
		return context.runMandatoryValidationForSaveData(saveData, [variantId]).then(
			() => true,
			() => false,
		);
	}

	function nameMessages() {
		const path = `$.variants[${UmbDataPathVariantQuery(variantId)}].name`;
		return validation.messages.getMessages().filter((x) => x.path === path);
	}

	it('marks the name as required when the new blueprint has no name', async () => {
		context.setName('', variantId);

		expect(await runMandatoryValidation()).to.be.false;
		expect(nameMessages().length).to.equal(1);
	});

	it('passes when the variant has a name', async () => {
		context.setName('Named', variantId);

		expect(await runMandatoryValidation()).to.be.true;
		expect(nameMessages().length).to.equal(0);
	});
});
