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

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);
	entityContext = new UmbEntityContext(this);
	ancestorsContext = new UmbAncestorsEntityContext(this);

	constructor() {
		super();
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
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
}

describe('UmbDocumentUserPermissionCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let condition: UmbDocumentUserPermissionCondition;

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

			// We expect to find the read permission on the current entity
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: (permitted) => {
					expect(permitted).to.be.true;
					done();
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

			// We expect to find the read permission on the ancestor
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: (permitted) => {
					expect(permitted).to.be.true;
					done();
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

			// We expect to find the read permission in the fallback permissions
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: (permitted) => {
					expect(permitted).to.be.true;
					done();
				},
			});
		});
	});
});
