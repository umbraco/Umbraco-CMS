import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbAncestorsEntityContext, UmbEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbElementUserPermissionCondition } from './element-user-permission.condition.js';
import { UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UMB_USER_PERMISSION_ELEMENT_CREATE, UMB_USER_PERMISSION_ELEMENT_READ } from '../constants.js';
import { UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';

@customElement('test-controller-host-element-permission')
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

describe('UmbElementUserPermissionCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let condition: UmbElementUserPermissionCondition;

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

	describe('Permissions granted on an ancestor folder', () => {
		const folderAncestor = {
			unique: 'permissions-folder-with-element-read-id',
			entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
		};

		it('applies the verbs of the closest ancestor folder to a descendant element', (done) => {
			hostElement.setEntity({ unique: 'descendant-element-id', entityType: UMB_ELEMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([folderAncestor]);

			condition = new UmbElementUserPermissionCondition(hostElement, {
				host: hostElement,
				config: { alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS, allOf: [UMB_USER_PERMISSION_ELEMENT_READ] },
				onChange: () => {
					expect(condition.permitted).to.be.true;
					condition.hostDisconnected();
					done();
				},
			});
		});

		it('denies a verb the closest ancestor folder does not grant', (done) => {
			hostElement.setEntity({ unique: 'descendant-element-id', entityType: UMB_ELEMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([folderAncestor]);

			condition = new UmbElementUserPermissionCondition(hostElement, {
				host: hostElement,
				config: { alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS, allOf: [UMB_USER_PERMISSION_ELEMENT_CREATE] },
				onChange: () => {},
			});

			// The onChange callback is not called when the condition is false, so we need to wait and check manually
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});

		it('denies element verbs when the closest ancestor folder only grants folder verbs', (done) => {
			hostElement.setEntity({ unique: 'descendant-element-id', entityType: UMB_ELEMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([
				{ unique: 'permissions-folder-without-element-verbs-id', entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE },
			]);

			condition = new UmbElementUserPermissionCondition(hostElement, {
				host: hostElement,
				config: { alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS, allOf: [UMB_USER_PERMISSION_ELEMENT_READ] },
				onChange: () => {},
			});

			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});

		it('prefers a permission on the element itself over one on an ancestor folder', (done) => {
			hostElement.setEntity({ unique: 'permissions-element-read-only-id', entityType: UMB_ELEMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([folderAncestor]);

			condition = new UmbElementUserPermissionCondition(hostElement, {
				host: hostElement,
				config: { alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS, allOf: [UMB_USER_PERMISSION_ELEMENT_CREATE] },
				onChange: () => {},
			});

			// The element's own entry only grants read, so create granted by the ancestor must not apply
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});
	});
});
