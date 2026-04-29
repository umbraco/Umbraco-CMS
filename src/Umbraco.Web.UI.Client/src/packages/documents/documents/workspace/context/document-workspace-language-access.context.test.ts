import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_USER_GROUP_ENTITY_TYPE, UmbUserGroupDetailRepository } from '@umbraco-cms/backoffice/user-group';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import { UmbLanguageAccessWorkspaceContext } from '../../../../language/permissions/language-access.workspace.context.js';
import { UmbUserGroupDetailStore } from '../../../../user/user-group/repository/detail/user-group-detail.store.js';

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const ADMIN_USER_GROUP_ID = 'variant-documents-user-group-administrators-id';

const EN_US = UmbVariantId.Create({ culture: 'en-US', segment: null });
const DA = UmbVariantId.Create({ culture: 'da', segment: null });

describe('UmbDocumentWorkspaceContext (Language Access)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let workspaceContext: UmbDocumentWorkspaceContext;

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
		workspaceContext = new UmbDocumentWorkspaceContext(hostElement);
		new UmbLanguageAccessWorkspaceContext(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('variant document', () => {
		beforeEach(async () => {
			await workspaceContext.load(VARIANT_DOCUMENT_ID);
		});

		describe('admin user (hasAccessToAllLanguages: true)', () => {
			it('has no read-only rule for en-US', () => {
				expect(workspaceContext.readOnlyGuard.getIsPermittedForVariant(EN_US)).to.be.false;
			});

			it('has no read-only rule for da', () => {
				expect(workspaceContext.readOnlyGuard.getIsPermittedForVariant(DA)).to.be.false;
			});
		});

		describe('restricted to en-US only (hasAccessToAllLanguages: false)', () => {
			beforeEach(async () => {
				new UmbUserGroupDetailStore(hostElement);
				const repo = new UmbUserGroupDetailRepository(hostElement);

				const { data } = await repo.requestByUnique(ADMIN_USER_GROUP_ID);
				await repo.save({ ...data!, hasAccessToAllLanguages: false, languages: ['en-US'] });

				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityUpdatedEvent({ entityType: UMB_USER_GROUP_ENTITY_TYPE, unique: ADMIN_USER_GROUP_ID }),
				);

				// Wait for the debounced reload (100ms) and state propagation to complete.
				await new Promise<void>((resolve) => {
					const subscription = workspaceContext.readOnlyGuard.rules.subscribe((rules) => {
						if (rules.some((r) => String(r.unique) === 'UMB_LANGUAGE_PERMISSION_da')) {
							subscription.unsubscribe();
							resolve();
						}
					});
				});
			});

			it('has no read-only rule for en-US', () => {
				expect(workspaceContext.readOnlyGuard.getIsPermittedForVariant(EN_US)).to.be.false;
			});

			it('has a read-only rule for da', () => {
				expect(workspaceContext.readOnlyGuard.getIsPermittedForVariant(DA)).to.be.true;
			});
		});
	});

	describe('invariant document', () => {
		beforeEach(async () => {
			await workspaceContext.load(INVARIANT_DOCUMENT_ID);
		});

		it('adds no language permission rules to the guard', () => {
			const rules = workspaceContext.readOnlyGuard.getRules();
			const languageRules = rules.filter((r) => String(r.unique).startsWith('UMB_LANGUAGE_PERMISSION_'));
			expect(languageRules).to.have.lengthOf(0);
		});
	});
});
