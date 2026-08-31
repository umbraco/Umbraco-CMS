import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbDocumentUserPermissionCondition } from './document-user-permission.condition';
import { UmbAncestorsEntityContext, UmbEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import {
	UMB_DOCUMENT_ENTITY_TYPE,
	UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_DOCUMENT_READ,
} from '@umbraco-cms/backoffice/document';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);
	entityContext = new UmbEntityContext(this);
	ancestorsContext = new UmbAncestorsEntityContext(this);
	currentUserStore = new UmbCurrentUserStore(this);

	constructor() {
		super();
		new UmbNotificationContext(this);
	}

	async init() {
		await this.currentUserContext.load();
	}

	setEntity(entity: UmbEntityModel) {
		this.entityContext.setUnique(entity.unique);
		this.entityContext.setEntityType(entity.entityType);
	}

	setEntityAncestors(ancestors: Array<UmbEntityModel>) {
		this.ancestorsContext.setAncestors(ancestors);
	}

	setDocumentStartNodeAccess(hasDocumentRootAccess: boolean, documentStartNodeUniques: Array<string>) {
		this.currentUserStore.update({
			hasDocumentRootAccess,
			documentStartNodeUniques: documentStartNodeUniques.map((unique) => ({ unique })),
		});
	}
}

describe('UmbDocumentUserPermissionCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let condition: UmbDocumentUserPermissionCondition;

	before(async () => {
		await useMockSet('userPermissions');
	});

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Specific permissions', () => {
		it('should return true if a user has permissions', (done) => {
			// Sets the current entity data
			hostElement.setEntity({
				unique: 'permissions-document-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});

			// This entity does not have any ancestors.
			hostElement.setEntityAncestors([]);

			let callbackCount = 0;

			// We expect to find the read permission on the current entity
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});

	describe('Inherited permissions', () => {
		it('should inherit permissions from closest ancestor with specific permissions set', (done) => {
			// Sets the current entity data
			hostElement.setEntity({
				unique: 'permissions-document-1-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});

			// Sets the ancestors of the current entity. These are the ancestors that will be checked for permissions.
			hostElement.setEntityAncestors([{ unique: 'permissions-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE }]);

			let callbackCount = 0;

			// We expect to find the read permission on the ancestor
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});

	describe('Fallback Permissions', () => {
		it('should use the fallback permissions if no specific permissions are set for the entity or ancestors', (done) => {
			// Sets the current entity to a document without permissions
			hostElement.setEntity({
				unique: 'no-permissions-document-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});

			// Sets the ancestors of the current entity. These are the ancestors that will be checked for permissions.
			// This ancestor does not have any permissions either.
			hostElement.setEntityAncestors([
				{ unique: 'no-permissions-parent-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE },
			]);

			let callbackCount = 0;

			// We expect to find the read permission in the fallback permissions
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});

	describe('Start node access', () => {
		it('is permitted when the document is within the users start-node subtree', (done) => {
			// User without root access, scoped to the "permissions-document-id" subtree
			hostElement.setDocumentStartNodeAccess(false, ['permissions-document-id']);

			// A descendant of the start node
			hostElement.setEntity({
				unique: 'permissions-document-1-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});
			hostElement.setEntityAncestors([{ unique: 'permissions-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE }]);

			let callbackCount = 0;

			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});

		it('is not permitted when the document is outside the users start-node subtree', (done) => {
			// User without root access, scoped to the "permissions-document-id" subtree
			hostElement.setDocumentStartNodeAccess(false, ['permissions-document-id']);

			// A document outside the start-node subtree - fallback permissions would otherwise grant read
			hostElement.setEntity({
				unique: 'no-permissions-document-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});
			hostElement.setEntityAncestors([
				{ unique: 'no-permissions-parent-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE },
			]);

			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {},
			});

			// The onChange callback is not called when the condition is false, so we need to wait and check manually
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});

		it('is not permitted when the user has no start nodes and no root access', (done) => {
			hostElement.setDocumentStartNodeAccess(false, []);

			hostElement.setEntity({
				unique: 'permissions-document-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});
			hostElement.setEntityAncestors([]);

			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {},
			});

			// The onChange callback is not called when the condition is false, so we need to wait and check manually
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});

		it('is permitted with ignorerUserStartNodes:true even when outside start-node subtree', (done) => {
			hostElement.setDocumentStartNodeAccess(false, ['permissions-document-id']);
			hostElement.setEntity({ unique: 'no-permissions-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([
				{ unique: 'no-permissions-parent-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE },
			]);

			let callbackCount = 0;

			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
					ignorerUserStartNodes: true, // bypass start-node check
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});
});
